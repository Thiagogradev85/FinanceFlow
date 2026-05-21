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
        var allTimeNet = await transactions.GetAllTimeNetAsync(query.UserId, ct);

        return new TransactionsSummaryDto(
            query.Year, query.Month, income, expense, income - expense, allTimeNet);
    }
}
