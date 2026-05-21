using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Accounts.Application;

public sealed record DeleteAccountCommand(Guid Id, Guid UserId) : IRequest<Result>;

public sealed class DeleteAccountHandler(IAccountRepository accounts, IAccountsUnitOfWork uow)
    : IRequestHandler<DeleteAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteAccountCommand cmd, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(cmd.Id, ct);
        if (account is null || account.UserId != cmd.UserId)
            return Result.Failure(AppError.NotFound("account.not_found", "Conta não encontrada."));

        account.Archive();
        accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
