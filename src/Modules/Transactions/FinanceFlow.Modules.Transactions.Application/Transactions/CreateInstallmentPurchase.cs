using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record CreateInstallmentPurchaseCommand(
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    decimal InstallmentAmount,
    int InstallmentCount,
    DateOnly FirstOccurredOn,
    string Description,
    string Currency = "BRL") : IRequest<Result<IReadOnlyList<TransactionDto>>>;

public sealed class CreateInstallmentPurchaseValidator : AbstractValidator<CreateInstallmentPurchaseCommand>
{
    public CreateInstallmentPurchaseValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.InstallmentAmount).GreaterThan(0);
        RuleFor(x => x.InstallmentCount).GreaterThanOrEqualTo(2)
            .WithMessage("Parcelamento exige ao menos 2 parcelas.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateInstallmentPurchaseHandler(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ITransactionsUnitOfWork uow,
    IEventBus eventBus) : IRequestHandler<CreateInstallmentPurchaseCommand, Result<IReadOnlyList<TransactionDto>>>
{
    public async Task<Result<IReadOnlyList<TransactionDto>>> Handle(CreateInstallmentPurchaseCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.CategoryId, ct);
        if (category is null || category.UserId != cmd.UserId)
            return AppError.NotFound("category.not_found", "Categoria não encontrada.");

        // Compra parcelada é sempre despesa — a categoria precisa ser de despesa.
        if (category.Kind != CategoryKind.Expense)
            return AppError.Validation("installment.category_must_be_expense",
                "Compra parcelada precisa de uma categoria de despesa.");

        var created = Transaction.CreateInstallmentPurchase(
            cmd.UserId, cmd.AccountId, cmd.CategoryId,
            cmd.InstallmentAmount, cmd.InstallmentCount, cmd.Currency, cmd.FirstOccurredOn, cmd.Description);
        if (created.IsFailure)
            return created.Error!;

        var installments = created.Value;

        // As N parcelas salvam na MESMA transação SQL (UnitOfWork) — ou todas, ou nenhuma.
        await transactions.AddRangeAsync(installments, ct);
        await uow.SaveChangesAsync(ct);

        // Publica os eventos só DEPOIS do commit. Fase 1: IEventBus loga; Fase 2: vira Kafka.
        foreach (var installment in installments)
        {
            foreach (var domainEvent in installment.DomainEvents)
                await eventBus.PublishAsync(domainEvent, ct);
            installment.ClearDomainEvents();
        }

        IReadOnlyList<TransactionDto> dtos = installments.Select(t => t.ToDto()).ToList();
        return Result<IReadOnlyList<TransactionDto>>.Success(dtos);
    }
}
