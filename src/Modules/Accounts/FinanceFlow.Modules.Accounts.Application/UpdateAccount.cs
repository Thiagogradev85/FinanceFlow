using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Accounts.Application;

public sealed record UpdateAccountCommand(
    Guid Id,
    Guid UserId,
    string Name,
    decimal OpeningBalance) : IRequest<Result<AccountDto>>;

public sealed class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
    }
}

public sealed class UpdateAccountHandler(IAccountRepository accounts, IAccountsUnitOfWork uow)
    : IRequestHandler<UpdateAccountCommand, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(UpdateAccountCommand cmd, CancellationToken ct)
    {
        var account = await accounts.GetByIdAsync(cmd.Id, ct);
        if (account is null || account.UserId != cmd.UserId)
            return AppError.NotFound("account.not_found", "Conta não encontrada.");

        var update = account.UpdateDetails(cmd.Name, cmd.OpeningBalance);
        if (update.IsFailure)
            return update.Error!;

        accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return account.ToDto();
    }
}
