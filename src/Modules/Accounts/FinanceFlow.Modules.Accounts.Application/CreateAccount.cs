using FinanceFlow.Modules.Accounts.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Accounts.Application;

public sealed record CreateAccountCommand(
    Guid UserId,
    string Name,
    AccountType Type,
    string Currency = "BRL",
    decimal OpeningBalance = 0) : IRequest<Result<AccountDto>>;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateAccountHandler(IAccountRepository accounts, IAccountsUnitOfWork uow)
    : IRequestHandler<CreateAccountCommand, Result<AccountDto>>
{
    public async Task<Result<AccountDto>> Handle(CreateAccountCommand cmd, CancellationToken ct)
    {
        var result = Account.Create(cmd.UserId, cmd.Name, cmd.Type, cmd.Currency, cmd.OpeningBalance);
        if (result.IsFailure)
            return result.Error!;

        // Primeira conta do usuário vira principal automaticamente — sempre há uma padrão.
        var existing = await accounts.ListByUserAsync(cmd.UserId, ct);
        if (existing.Count == 0)
            result.Value.MakePrimary();

        await accounts.AddAsync(result.Value, ct);
        await uow.SaveChangesAsync(ct);

        return result.Value.ToDto();
    }
}
