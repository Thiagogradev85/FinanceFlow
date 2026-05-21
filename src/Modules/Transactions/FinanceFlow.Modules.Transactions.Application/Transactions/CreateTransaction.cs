using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record CreateTransactionCommand(
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateOnly OccurredOn,
    string Description,
    string Currency = "BRL") : IRequest<Result<TransactionDto>>;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Type).Must(t => t is TransactionType.Income or TransactionType.Expense)
            .WithMessage("Use o endpoint de transferência para Type=Transfer.");
    }
}

public sealed class CreateTransactionHandler(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ITransactionsUnitOfWork uow,
    IEventBus eventBus) : IRequestHandler<CreateTransactionCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.CategoryId, ct);
        if (category is null || category.UserId != cmd.UserId)
            return AppError.NotFound("category.not_found", "Categoria não encontrada.");

        // Regra ENTRE entidades (não cabe dentro de uma só): o tipo da transação tem que
        // bater com o tipo da categoria. Por isso vive no handler, não na entidade.
        var expectedKind = cmd.Type == TransactionType.Income ? CategoryKind.Income : CategoryKind.Expense;
        if (category.Kind != expectedKind)
            return AppError.Validation("transaction.category_kind_mismatch",
                "O tipo da transação não combina com o tipo da categoria.");

        var created = Transaction.CreateIncomeOrExpense(
            cmd.UserId, cmd.AccountId, cmd.CategoryId, cmd.Type,
            cmd.Amount, cmd.Currency, cmd.OccurredOn, cmd.Description);
        if (created.IsFailure)
            return created.Error!;

        var transaction = created.Value;
        await transactions.AddAsync(transaction, ct);
        await uow.SaveChangesAsync(ct);

        // Publica os eventos de domínio DEPOIS do commit. Fase 1: IEventBus só loga.
        // Fase 2: troca o registro para Kafka e um consumer reage de forma assíncrona.
        foreach (var domainEvent in transaction.DomainEvents)
            await eventBus.PublishAsync(domainEvent, ct);
        transaction.ClearDomainEvents();

        return transaction.ToDto();
    }
}
