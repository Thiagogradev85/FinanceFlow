using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record CreateTransferCommand(
    Guid UserId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateOnly OccurredOn,
    string Description,
    string Currency = "BRL") : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class CreateTransferValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.FromAccountId).NotEmpty();
        RuleFor(x => x.ToAccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateTransferHandler(
    ITransactionRepository transactions,
    ITransactionsUnitOfWork uow,
    IEventBus eventBus) : IRequestHandler<CreateTransferCommand, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(CreateTransferCommand cmd, CancellationToken ct)
    {
        var created = Transaction.CreateTransferPair(
            cmd.UserId, cmd.FromAccountId, cmd.ToAccountId, cmd.Amount, cmd.Currency, cmd.OccurredOn, cmd.Description);
        if (created.IsFailure)
            return created.Error!;

        var (outLeg, inLeg) = created.Value;

        // As duas pernas salvam na MESMA transação SQL (UnitOfWork) — ou as duas, ou nenhuma.
        await transactions.AddRangeAsync([outLeg, inLeg], ct);
        await uow.SaveChangesAsync(ct);

        // Publica os eventos só DEPOIS do commit. Fase 1: IEventBus loga; Fase 2: vira Kafka.
        foreach (var leg in new[] { outLeg, inLeg })
        {
            foreach (var domainEvent in leg.DomainEvents)
                await eventBus.PublishAsync(domainEvent, ct);
            leg.ClearDomainEvents();
        }

        IReadOnlyList<TransactionDto> dtos = [outLeg.ToDto(), inLeg.ToDto()];
        return Result<IReadOnlyList<TransactionDto>>.Success(dtos);
    }
}
