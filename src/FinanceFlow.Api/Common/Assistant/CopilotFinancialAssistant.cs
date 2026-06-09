using System.ComponentModel;
using FinanceFlow.SharedKernel;
using GitHub.Copilot.SDK;
using MediatR;
using Microsoft.Extensions.AI;

namespace FinanceFlow.Api.Common.Assistant;

/// <summary>
/// Implementação do assistente usando o GitHub Copilot SDK (function calling).
/// Roda LOCAL: depende do Copilot CLI instalado e logado (copilot /login). Para deploy em
/// nuvem, trocar pelo <see cref="ClaudeFinancialAssistant"/> (que só precisa de uma API key).
/// </summary>
public sealed class CopilotFinancialAssistant(
    CopilotClient client,
    ISender sender,
    IClock clock,
    ILogger<CopilotFinancialAssistant> logger) : IFinancialAssistant
{
    public async Task<Result<AssistantReply>> InterpretAsync(string message, Guid userId, CancellationToken ct)
    {
        var ctxResult = await AssistantProposalFactory.LoadContextAsync(sender, userId, ct);
        if (ctxResult.IsFailure) return ctxResult.Error!;
        var ctx = ctxResult.Value;

        // O modelo deposita aqui o que interpretou. A tool NÃO grava nada — só captura.
        ParsedProposal? captured = null;

        // Os [Description] viram o schema que o modelo lê para preencher os argumentos.
        [Description("Registra a proposta de lançamento financeiro interpretada da frase do usuário.")]
        string RegistrarProposta(
            [Description("Valor em reais, sempre positivo. Ex: 20.50")] decimal valor,
            [Description("Nome EXATO de uma das categorias disponíveis na instrução de sistema.")] string categoria,
            [Description("Data no formato AAAA-MM-DD. Vazio = hoje.")] string? data,
            [Description("Descrição curta livre.")] string? descricao)
        {
            captured = new ParsedProposal(valor, categoria, data, descricao);
            return "ok, proposta registrada";
        }

        var hoje = DateOnly.FromDateTime(clock.UtcNow).ToString("yyyy-MM-dd");
        var system = $"""
            Você é o assistente de lançamentos do app FinanceFlow. Interprete a frase do usuário
            sobre um gasto ou recebimento e chame a tool RegistrarProposta exatamente uma vez.

            Categorias disponíveis (use o nome EXATO; a categoria define se é receita ou despesa):
            {AssistantProposalFactory.BuildCategoryList(ctx.Categories)}

            Nunca invente categoria fora da lista. Datas relativas ("ontem"/"hoje") viram AAAA-MM-DD.
            Sem data na frase = deixe vazio (o app assume hoje).
            """;

        try
        {
            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                OnPermissionRequest = PermissionHandler.ApproveAll,
                SystemMessage = new SystemMessageConfig { Content = system },
                Tools = [AIFunctionFactory.Create(RegistrarProposta)],
            }, ct);

            await session.SendAndWaitAsync(new MessageOptions { Prompt = $"Hoje é {hoje}. Lançamento: {message}" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao chamar o Copilot SDK");
            return AppError.Domain("assistant.llm_error",
                "Não consegui falar com a IA agora. Confira se o Copilot CLI está instalado e logado (copilot /login).");
        }

        if (captured is null)
            return AppError.Domain("assistant.no_proposal",
                "Não entendi um lançamento nessa frase. Tente algo como \"20 comida\".");

        return AssistantProposalFactory.Build(captured, ctx, clock);
    }
}
