using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;

namespace FinanceFlow.Api.Common.Assistant;

/// <summary>
/// Proposta de lançamento que o assistente interpretou da frase do usuário.
/// NÃO é uma transação ainda — o usuário confirma no front, que então chama
/// POST /api/transactions. O LLM só interpreta e propõe; quem grava é o domínio.
/// </summary>
public sealed record TransactionProposal(
    Guid AccountId,
    string AccountName,
    Guid CategoryId,
    string CategoryName,
    TransactionType Type,
    decimal Amount,
    DateOnly OccurredOn,
    string Description);

/// <summary>Resposta do assistente: uma frase para o chat + (opcional) a proposta a confirmar.</summary>
public sealed record AssistantReply(string Reply, TransactionProposal? Proposal);

/// <summary>Uma linha do breakdown de categorias, usada como entrada para a análise com IA.</summary>
public sealed record CategorySummaryForAnalysis(
    string Name,
    string Icon,
    decimal Total,
    decimal PercentOfExpense);

/// <summary>Contexto financeiro do mês que alimenta a análise narrativa da IA.</summary>
public sealed record FinancialAnalysisInput(
    int Year,
    int Month,
    decimal Income,
    decimal Expense,
    decimal Net,
    IReadOnlyList<CategorySummaryForAnalysis> Breakdown);

/// <summary>
/// Abstração fina sobre o modelo de IA. Isola o SDK (preview/trocável) do resto do app:
/// hoje é Anthropic; trocar de provedor mexe só na implementação, não nos endpoints.
/// </summary>
public interface IFinancialAssistant
{
    Task<Result<AssistantReply>> InterpretAsync(string message, Guid userId, CancellationToken ct);

    /// <summary>
    /// Gera 2-3 frases de análise financeira em pt-BR a partir do resumo do mês.
    /// Deve retornar erro gracioso quando o LLM não estiver disponível.
    /// </summary>
    Task<Result<string>> AnalyzeAsync(FinancialAnalysisInput input, CancellationToken ct);
}
