using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record GetAccountsNetQuery(Guid UserId, DateOnly AsOf) : IRequest<Result<IReadOnlyDictionary<Guid, decimal>>>;

public sealed class GetAccountsNetHandler(ITransactionRepository transactions)
    : IRequestHandler<GetAccountsNetQuery, Result<IReadOnlyDictionary<Guid, decimal>>>
{
    public async Task<Result<IReadOnlyDictionary<Guid, decimal>>> Handle(GetAccountsNetQuery query, CancellationToken ct)
    {
        var nets = await transactions.GetAllTimeNetByAccountsAsync(query.UserId, query.AsOf, ct);
        return Result<IReadOnlyDictionary<Guid, decimal>>.Success(nets);
    }
}
