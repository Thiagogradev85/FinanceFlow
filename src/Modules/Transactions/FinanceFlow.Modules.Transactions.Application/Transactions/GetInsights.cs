using System.Globalization;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record GetInsightsQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<IReadOnlyList<string>>>;

public sealed class GetInsightsHandler(ITransactionRepository transactions)
    : IRequestHandler<GetInsightsQuery, Result<IReadOnlyList<string>>>
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<Result<IReadOnlyList<string>>> Handle(GetInsightsQuery query, CancellationToken ct)
    {
        var (income, expense) = await transactions.GetMonthTotalsAsync(query.UserId, query.Year, query.Month, ct);
        var breakdown = await transactions.GetCategoryBreakdownAsync(query.UserId, query.Year, query.Month, ct);

        var insights = new List<string>();

        if (income == 0 && expense == 0)
        {
            insights.Add("Nenhuma movimentação registrada neste mês.");
            return Result<IReadOnlyList<string>>.Success(insights);
        }

        var net = income - expense;

        // Regra 1: gastou mais do que ganhou
        if (net < 0)
        {
            insights.Add($"Você gastou mais do que ganhou este mês (saldo: {Brl(net)}).");
        }
        else if (income > 0)
        {
            // Regra 2: taxa de poupança
            var savingsRate = Math.Round(net / income * 100, 1);
            insights.Add($"Você economizou {savingsRate}% da sua receita ({Brl(net)}).");
        }

        if (breakdown.Count > 0)
        {
            var totalExpense = breakdown.Sum(r => r.Total);

            // Regra 3: categoria que domina os gastos (≥ 40%)
            var top = breakdown.OrderByDescending(r => r.Total).First();
            if (totalExpense > 0)
            {
                var topPct = Math.Round(top.Total / totalExpense * 100, 1);
                if (topPct >= 40)
                    insights.Add($"{top.CategoryName} concentrou {topPct}% dos seus gastos ({Brl(top.Total)}).");
            }

            // Regra 4: categoria que pesou na receita (≥ 30%)
            if (income > 0)
            {
                foreach (var cat in breakdown.OrderByDescending(r => r.Total))
                {
                    var catPct = Math.Round(cat.Total / income * 100, 1);
                    if (catPct >= 30)
                        insights.Add($"{cat.CategoryName} consumiu {catPct}% da sua receita do mês.");
                }
            }
        }

        // Regra 5: parcelas comprometidas no próximo mês
        var nextMonthStart = query.Month == 12
            ? new DateOnly(query.Year + 1, 1, 1)
            : new DateOnly(query.Year, query.Month + 1, 1);
        var nextMonthEnd = nextMonthStart.AddMonths(1);

        var commitments = await transactions.GetMonthlyCommittedInstallmentsAsync(
            query.UserId, nextMonthStart, nextMonthEnd, ct);

        var nextCommitment = commitments.FirstOrDefault();
        if (nextCommitment is not null && nextCommitment.Amount > 0)
        {
            if (income > 0)
            {
                var commitPct = Math.Round(nextCommitment.Amount / income * 100, 1);
                insights.Add($"Você tem {Brl(nextCommitment.Amount)} em parcelas no próximo mês ({commitPct}% da sua receita).");
            }
            else
            {
                insights.Add($"Você tem {Brl(nextCommitment.Amount)} em parcelas no próximo mês.");
            }
        }

        if (insights.Count == 0)
            insights.Add("Suas finanças estão equilibradas este mês. Continue assim!");

        return Result<IReadOnlyList<string>>.Success(insights);
    }

    private static string Brl(decimal value) =>
        value.ToString("C", PtBr);
}
