using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record UpdateTransactionCommand(
    Guid Id,
    Guid UserId,
    Guid AccountId,
    Guid CategoryId,
    TransactionType Type,
    decimal Amount,
    DateOnly OccurredOn,
    string Description,
    string Currency = "BRL") : IRequest<Result<TransactionDto>>;

public sealed class UpdateTransactionValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Type).Must(t => t is TransactionType.Income or TransactionType.Expense)
            .WithMessage("Use o endpoint de transferência para Type=Transfer.");
    }
}

public sealed class UpdateTransactionHandler(
    ITransactionRepository transactions,
    ICategoryRepository categories,
    ITransactionsUnitOfWork uow) : IRequestHandler<UpdateTransactionCommand, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(UpdateTransactionCommand cmd, CancellationToken ct)
    {
        var transaction = await transactions.GetByIdAsync(cmd.Id, ct);
        if (transaction is null || transaction.UserId != cmd.UserId)
            return AppError.NotFound("transaction.not_found", "Transação não encontrada.");

        var category = await categories.GetByIdAsync(cmd.CategoryId, ct);
        if (category is null || category.UserId != cmd.UserId)
            return AppError.NotFound("category.not_found", "Categoria não encontrada.");

        var expectedKind = cmd.Type == TransactionType.Income ? CategoryKind.Income : CategoryKind.Expense;
        if (category.Kind != expectedKind)
            return AppError.Validation("transaction.category_kind_mismatch",
                "O tipo da transação não combina com o tipo da categoria.");

        var edit = transaction.EditIncomeOrExpense(
            cmd.AccountId, cmd.CategoryId, cmd.Type, cmd.Amount, cmd.Currency, cmd.OccurredOn, cmd.Description);
        if (edit.IsFailure)
            return edit.Error!;

        transactions.Update(transaction);
        await uow.SaveChangesAsync(ct);

        return transaction.ToDto();
    }
}
