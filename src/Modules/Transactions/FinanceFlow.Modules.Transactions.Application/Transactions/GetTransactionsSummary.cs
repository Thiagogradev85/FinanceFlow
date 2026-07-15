using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record GetTransactionsSummaryQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<TransactionsSummaryDto>>;

public sealed class GetTransactionsSummaryHandler(ITransactionRepository transactions)
    : IRequestHandler<GetTransactionsSummaryQuery, Result<TransactionsSummaryDto>>
{
    public async Task<Result<TransactionsSummaryDto>> Handle(GetTransactionsSummaryQuery query, CancellationToken ct)
    {
        var (income, expense) = await transactions.GetMonthTotalsAsync(query.UserId, query.Year, query.Month, ct);

        var lastDayOfPrevMonth = new DateOnly(query.Year, query.Month, 1).AddDays(-1);
        var prevNet = await transactions.GetAllTimeNetAsync(query.UserId, lastDayOfPrevMonth, ct);

        if (prevNet > 0) income += prevNet;
        else if (prevNet < 0) expense += Math.Abs(prevNet);

        // Saldo PREVISTO até o fim do mês de referência: net de tudo com OccurredOn <= último dia do mês.
        // Mês futuro → projeta o impacto das parcelas; mês corrente → equivale ao saldo de hoje
        // (não há nada lançado entre hoje e o fim do mês); mês passado → como o saldo estava lá.
        var endOfMonth = new DateOnly(query.Year, query.Month, 1).AddMonths(1).AddDays(-1);
        var projectedNet = await transactions.GetAllTimeNetAsync(query.UserId, endOfMonth, ct);

        return new TransactionsSummaryDto(
            query.Year, query.Month, income, expense, income - expense, projectedNet);
    }
}
