using FinanceFlow.Api.Common;
using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Transactions.Application.Transactions;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Api.Endpoints;

public static class DashboardEndpoints
{
    public sealed record DashboardDto(
        int Year,
        int Month,
        decimal TotalBalance,
        decimal OpeningBalance,   // saldo inicial somado das contas
        decimal TransactionsNet,  // entradas − saídas de todas as transações
        decimal MonthIncome,
        decimal MonthExpense,
        decimal MonthNet);

    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        // O dashboard COMPÕE dois módulos no nível da API (não há chamada módulo→módulo):
        // saldo de abertura (Accounts) + net de todas as transações (Transactions).
        app.MapGet("/api/dashboard", async (int? year, int? month, IClock clock, ISender sender, CancellationToken ct) =>
        {
            var now = clock.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var opening = await sender.Send(new GetAccountsOpeningTotalQuery(DemoUser.Id), ct);
            var summary = await sender.Send(new GetTransactionsSummaryQuery(DemoUser.Id, y, m), ct);

            var totalBalance = opening.Value + summary.Value.AllTimeNet;
            var dto = new DashboardDto(
                y, m, totalBalance,
                opening.Value, summary.Value.AllTimeNet,
                summary.Value.MonthIncome, summary.Value.MonthExpense, summary.Value.MonthNet);

            return Results.Ok(dto);
        }).WithTags("Dashboard");

        app.MapGet("/api/dashboard/breakdown", async (
            int? year, int? month,
            IClock clock, ISender sender, CancellationToken ct) =>
        {
            var now = clock.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var result = await sender.Send(new GetCategoryBreakdownQuery(DemoUser.Id, y, m), ct);
            return result.ToHttp();
        }).WithTags("Dashboard");

        app.MapGet("/api/insights", async (
            int? year, int? month,
            IClock clock, ISender sender, CancellationToken ct) =>
        {
            var now = clock.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var result = await sender.Send(new GetInsightsQuery(DemoUser.Id, y, m), ct);
            return result.ToHttp();
        }).WithTags("Insights");
    }
}
