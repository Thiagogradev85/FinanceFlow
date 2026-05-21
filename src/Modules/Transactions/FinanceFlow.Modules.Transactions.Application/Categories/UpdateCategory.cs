using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Categories;

public sealed record UpdateCategoryCommand(
    Guid Id,
    Guid UserId,
    string Name,
    string Color = "#888888",
    string Icon = "tag") : IRequest<Result<CategoryDto>>;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
    }
}

public sealed class UpdateCategoryHandler(ICategoryRepository categories, ITransactionsUnitOfWork uow)
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand cmd, CancellationToken ct)
    {
        var category = await categories.GetByIdAsync(cmd.Id, ct);
        if (category is null || category.UserId != cmd.UserId)
            return AppError.NotFound("category.not_found", "Categoria não encontrada.");

        category.Update(cmd.Name, cmd.Color, cmd.Icon);
        categories.Update(category);
        await uow.SaveChangesAsync(ct);

        return category.ToDto();
    }
}
