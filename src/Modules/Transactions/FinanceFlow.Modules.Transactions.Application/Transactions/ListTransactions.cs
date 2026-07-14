using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

// Sem Year/Month → últimas N transações de TODOS os meses (default da aba Transações,
// pra você ver e corrigir o que lançou). Com Year+Month → filtra aquele mês e prepend
// uma entrada virtual "Saldo mês anterior" no dia 1 (carry-forward do saldo acumulado).
public sealed record ListTransactionsQuery(Guid UserId, int? Year = null, int? Month = null, int Take = 200)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class ListTransactionsHandler(ITransactionRepository transactions)
    : IRequestHandler<ListTransactionsQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(ListTransactionsQuery query, CancellationToken ct)
    {
        if (query.Year is not int year || query.Month is not int month)
        {
            var recent = await transactions.ListRecentByUserAsync(query.UserId, query.Take, ct);
            IReadOnlyList<TransactionDto> recentDtos = recent.Select(t => t.ToDto()).ToList();
            return Result<IReadOnlyList<TransactionDto>>.Success(recentDtos);
        }

        var items = await transactions.ListByUserAndMonthAsync(query.UserId, year, month, ct);
        var dtos = items.Select(t => t.ToDto()).ToList();

        // Saldo acumulado até o último dia do mês anterior (carry-forward).
        var lastDayOfPrevMonth = new DateOnly(year, month, 1).AddDays(-1);
        var prevNet = await transactions.GetAllTimeNetAsync(query.UserId, lastDayOfPrevMonth, ct);

        if (prevNet != 0m)
        {
            var carryForward = new TransactionDto(
                Id: Guid.Empty,
                AccountId: Guid.Empty,
                CategoryId: null,
                Type: prevNet > 0 ? (int)TransactionType.Income : (int)TransactionType.Expense,
                Direction: prevNet > 0 ? (int)TransactionDirection.Inflow : (int)TransactionDirection.Outflow,
                Amount: Math.Abs(prevNet),
                Currency: "BRL",
                OccurredOn: new DateOnly(year, month, 1),
                Description: "Saldo mês anterior",
                IsCarryForward: true);

            dtos.Insert(0, carryForward);
        }

        return Result<IReadOnlyList<TransactionDto>>.Success(dtos);
    }
}
