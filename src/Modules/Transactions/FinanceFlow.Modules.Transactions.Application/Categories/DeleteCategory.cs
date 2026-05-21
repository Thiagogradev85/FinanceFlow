using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Categories;

public sealed record DeleteCategoryCommand(Guid Id, Guid UserId) : IRequest<Result>;

public sealed class DeleteCategoryHandler(ICategoryRepository categories, ITransactionsUnitOfWork uow)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.Id, ct);
        if (category is null || category.UserId != cmd.UserId)
            return Result.Failure(AppError.NotFound("category.not_found", "Categoria não encontrada."));

        // Arquiva em vez de apagar: transações antigas mantêm a referência sem quebrar o histórico.
        category.Archive();
        categories.Update(category);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
