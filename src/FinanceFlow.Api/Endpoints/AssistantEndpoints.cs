using FinanceFlow.Api.Common;
using FinanceFlow.Api.Common.Assistant;
using FinanceFlow.Modules.Transactions.Application.Transactions;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Endpoints;

public static class AssistantEndpoints
{
    public sealed record InterpretRequest(string Message);
    public sealed record AnalyzeRequest(int? Year, int? Month);

    public static void MapAssistantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assistant").WithTags("Assistant");

        // Interpreta a frase e devolve uma PROPOSTA (não grava). O front confirma e chama POST /api/transactions.
        group.MapPost("/interpret", async (InterpretRequest req, IFinancialAssistant assistant, CancellationToken ct) =>
            (await assistant.InterpretAsync(req.Message ?? "", DemoUser.Id, ct)).ToHttp());

        // Gera narrativa de análise financeira com IA para o mês/ano informados.
        app.MapPost("/api/insights/analyze", async (
            AnalyzeRequest req,
            IFinancialAssistant assistant,
            IClock clock,
            ISender sender,
            CancellationToken ct) =>
        {
            var now = clock.UtcNow;
            var y = req.Year ?? now.Year;
            var m = req.Month ?? now.Month;

            var summaryResult = await sender.Send(new GetTransactionsSummaryQuery(DemoUser.Id, y, m), ct);
            var breakdownResult = await sender.Send(new GetCategoryBreakdownQuery(DemoUser.Id, y, m), ct);

            if (summaryResult.IsFailure) return summaryResult.ToHttp();
            if (breakdownResult.IsFailure) return breakdownResult.ToHttp();

            var s = summaryResult.Value!;
            var b = breakdownResult.Value ?? [];

            var input = new FinancialAnalysisInput(y, m,
                s.MonthIncome, s.MonthExpense, s.MonthNet,
                b.Select(c => new CategorySummaryForAnalysis(c.CategoryName, c.Icon, c.Total, c.PercentOfExpense))
                 .ToList());

            return (await assistant.AnalyzeAsync(input, ct)).ToHttp();
        }).WithTags("Insights");
    }
}
