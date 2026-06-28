using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Accounts.Application;

public sealed record SetPrimaryAccountCommand(Guid Id, Guid UserId) : IRequest<Result<AccountDto>>;

/// <summary>
/// Marca uma conta como principal. A invariante "só uma principal por usuário" cruza vários
/// agregados Account, então é garantida aqui: zera a principal atual e marca a nova no mesmo
/// SaveChanges (uma transação). Conta arquivada não pode virar principal.
/// </summary>
public sealed class SetPrimaryAccountHandler(IAccountRepository accounts, IAccountsUnitOfWork uow)
    : IRequestHandler<SetPrimaryAccountCommand, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(SetPrimaryAccountCommand cmd, CancellationToken ct)
    {
        var userAccounts = await accounts.ListByUserAsync(cmd.UserId, ct);

        var target = userAccounts.FirstOrDefault(a => a.Id == cmd.Id);
        if (target is null)
            return AppError.NotFound("account.not_found", "Conta não encontrada.");

        if (target.IsPrimary)
            return target.ToDto();

        foreach (var current in userAccounts.Where(a => a.IsPrimary))
        {
            current.ClearPrimary();
            accounts.Update(current);
        }

        target.MakePrimary();
        accounts.Update(target);

        await uow.SaveChangesAsync(ct);
        return target.ToDto();
    }
}
