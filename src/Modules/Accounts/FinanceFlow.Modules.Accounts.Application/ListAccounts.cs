using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Accounts.Application;

public sealed record ListAccountsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<AccountDto>>>;

public sealed class ListAccountsHandler(IAccountRepository accounts)
    : IRequestHandler<ListAccountsQuery, Result<IReadOnlyList<AccountDto>>>
{
    public async Task<Result<IReadOnlyList<AccountDto>>> Handle(ListAccountsQuery query, CancellationToken ct)
    {
        var items = await accounts.ListByUserAsync(query.UserId, ct);
        IReadOnlyList<AccountDto> dtos = items.Select(a => a.ToDto()).ToList();
        return Result<IReadOnlyList<AccountDto>>.Success(dtos);
    }
}

public sealed record GetAccountsOpeningTotalQuery(Guid UserId) : IRequest<Result<decimal>>;

public sealed class GetAccountsOpeningTotalHandler(IAccountRepository accounts)
    : IRequestHandler<GetAccountsOpeningTotalQuery, Result<decimal>>
{
    public async Task<Result<decimal>> Handle(GetAccountsOpeningTotalQuery query, CancellationToken ct)
    {
        var total = await accounts.GetOpeningTotalAsync(query.UserId, ct);
        return Result<decimal>.Success(total);
    }
}
