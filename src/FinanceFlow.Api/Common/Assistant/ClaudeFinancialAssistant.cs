using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Common.Assistant;

/// <summary>
/// Implementação do assistente usando a Messages API da Anthropic com tool use forçado.
/// Funciona local e em nuvem (só precisa de ANTHROPIC_API_KEY) — é o caminho do DEPLOY.
/// </summary>
public sealed class ClaudeFinancialAssistant(
    AnthropicClient client,
    ISender sender,
    IClock clock,
    ILogger<ClaudeFinancialAssistant> logger) : IFinancialAssistant
{
    private const string ToolName = "registrar_proposta";

    public async Task<Result<AssistantReply>> InterpretAsync(string message, Guid userId, CancellationToken ct)
    {
        var ctxResult = await AssistantProposalFactory.LoadContextAsync(sender, userId, ct);
        if (ctxResult.IsFailure) return ctxResult.Error!;
        var ctx = ctxResult.Value;

        // tool_choice FORÇA a tool: o modelo é obrigado a devolver os campos estruturados.
        ToolUseBlock? toolUse = null;
        try
        {
            var response = await client.Messages.Create(BuildRequest(message, ctx.Categories));
            foreach (var block in response.Content)
                if (block.TryPickToolUse(out var t)) { toolUse = t; break; }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao chamar a API da Anthropic");
            return AppError.Domain("assistant.llm_error",
                "Não consegui falar com a IA agora. Confira a variável ANTHROPIC_API_KEY e tente de novo.");
        }

        if (toolUse is null)
            return AppError.Domain("assistant.no_proposal",
                "Não entendi um lançamento nessa frase. Tente algo como \"20 comida\".");

        ParsedProposal parsed;
        try { parsed = ParseToolInput(toolUse.Input); }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Input da tool em formato inesperado");
            return AppError.Domain("assistant.bad_proposal", "Não consegui montar o lançamento. Tente reformular.");
        }

        return AssistantProposalFactory.Build(parsed, ctx, clock);
    }

    private MessageCreateParams BuildRequest(string message, IReadOnlyList<CategoryDto> categories)
    {
        var system = $"""
            Você é o assistente de lançamentos do app FinanceFlow. Interprete uma frase curta do
            usuário sobre um gasto ou recebimento e chame a tool {ToolName}.

            Categorias disponíveis (use o nome EXATO; a categoria define se é receita ou despesa):
            {AssistantProposalFactory.BuildCategoryList(categories)}

            Regras:
            - Nunca invente categoria fora da lista; escolha a mais próxima do sentido da frase.
            - Valor sempre positivo. Datas relativas ("ontem", "hoje") viram AAAA-MM-DD.
            - Se não houver data na frase, deixe o campo vazio (o app assume hoje).
            """;

        var hoje = DateOnly.FromDateTime(clock.UtcNow).ToString("yyyy-MM-dd");

        return new MessageCreateParams
        {
            Model = Model.ClaudeOpus4_8,
            MaxTokens = 512,
            Thinking = new ThinkingConfigDisabled(), // extração simples; sem thinking = mais rápido/barato
            // System é o prefixo estável (cacheável); a data, que muda todo dia, vai no turno do usuário.
            System = new List<TextBlockParam>
            {
                new() { Text = system, CacheControl = new CacheControlEphemeral() }
            },
            Tools =
            [
                new Tool
                {
                    Name = ToolName,
                    Description = "Registra a proposta de lançamento financeiro interpretada da frase do usuário.",
                    InputSchema = new()
                    {
                        Properties = new Dictionary<string, JsonElement>
                        {
                            ["valor"] = Schema(new { type = "number", description = "Valor em reais, sempre positivo." }),
                            ["categoria"] = Schema(new { type = "string", description = "Nome EXATO de uma das categorias disponíveis." }),
                            ["data"] = Schema(new { type = "string", description = "Data no formato AAAA-MM-DD. Vazio = hoje." }),
                            ["descricao"] = Schema(new { type = "string", description = "Descrição curta livre." }),
                        },
                        Required = ["valor", "categoria"],
                    },
                }
            ],
            ToolChoice = new ToolChoiceTool { Name = ToolName }, // OBRIGA o modelo a chamar a tool
            Messages = [new() { Role = Role.User, Content = $"Hoje é {hoje}. Lançamento: {message}" }],
        };
    }

    private static JsonElement Schema(object shape) => JsonSerializer.SerializeToElement(shape);

    private static ParsedProposal ParseToolInput(object input)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(input));
        var root = doc.RootElement;
        var valor = root.GetProperty("valor").GetDecimal();
        var categoria = root.GetProperty("categoria").GetString() ?? "";
        var data = root.TryGetProperty("data", out var d) ? d.GetString() : null;
        var descricao = root.TryGetProperty("descricao", out var de) ? de.GetString() : null;
        return new ParsedProposal(valor, categoria, data, descricao);
    }
}
