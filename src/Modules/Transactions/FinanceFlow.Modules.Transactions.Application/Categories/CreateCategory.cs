using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;
using FluentValidation;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Categories;

public sealed record CreateCategoryCommand(
    Guid UserId,
    string Name,
    CategoryKind Kind,
    string Color = "#888888",
    string Icon = "tag") : IRequest<Result<CategoryDto>>;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Kind).IsInEnum();
    }
}

public sealed class CreateCategoryHandler(ICategoryRepository categories, ITransactionsUnitOfWork uow)
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var result = Category.Create(cmd.UserId, cmd.Name, cmd.Kind, cmd.Color, cmd.Icon);
        if (result.IsFailure)
            return result.Error!;

        await categories.AddAsync(result.Value, ct);
        await uow.SaveChangesAsync(ct);

        return result.Value.ToDto();
    }
}
