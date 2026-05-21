using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record ListTransactionsQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class ListTransactionsHandler(ITransactionRepository transactions)
    : IRequestHandler<ListTransactionsQuery, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(ListTransactionsQuery query, CancellationToken ct)
    {
        var items = await transactions.ListByUserAndMonthAsync(query.UserId, query.Year, query.Month, ct);
        IReadOnlyList<TransactionDto> dtos = items.Select(t => t.ToDto()).ToList();
        return Result<IReadOnlyList<TransactionDto>>.Success(dtos);
    }
}
