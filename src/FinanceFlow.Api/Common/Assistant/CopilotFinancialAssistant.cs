using System.ComponentModel;
using System.Globalization;
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
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
    private static string Brl(decimal v) => v.ToString("C", PtBr);

    public async Task<Result<string>> AnalyzeAsync(FinancialAnalysisInput input, CancellationToken ct)
    {
        var catLines = input.Breakdown.Count > 0
            ? string.Join("\n", input.Breakdown.Select(c =>
                $"  - {c.Name}: {Brl(c.Total)} ({c.PercentOfExpense:F1}%)"))
            : "  (nenhuma despesa registrada)";

        string? captured = null;

        [Description("Entrega a análise financeira do mês ao usuário.")]
        string EntregarAnalise(
            [Description("Análise em 2-3 frases diretas em pt-BR com conselho prático.")] string analise)
        {
            captured = analise;
            return "ok";
        }

        const string system = "Você é um consultor financeiro pessoal. Analise os dados e chame EntregarAnalise com 2-3 frases diretas em pt-BR. Não repita os números — foque em conselhos práticos.";

        var prompt = $"""
            Mês {input.Month:00}/{input.Year}:
            - Receita: {Brl(input.Income)}
            - Gasto: {Brl(input.Expense)}
            - Saldo do mês: {Brl(input.Net)}
            Categorias de gasto:
            {catLines}
            """;

        try
        {
            await using var session = await client.CreateSessionAsync(new SessionConfig
            {
                OnPermissionRequest = PermissionHandler.ApproveAll,
                SystemMessage = new SystemMessageConfig { Content = system },
                Tools = [AIFunctionFactory.Create(EntregarAnalise)],
            }, ct);

            await session.SendAndWaitAsync(new MessageOptions { Prompt = prompt });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao gerar análise com Copilot");
            return AppError.Domain("assistant.llm_error",
                "Não foi possível gerar a análise agora. Verifique se o Copilot CLI está instalado e logado.");
        }

        return captured is not null
            ? Result<string>.Success(captured)
            : AppError.Domain("assistant.no_narrative", "A IA não retornou uma análise. Tente novamente.");
    }

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
