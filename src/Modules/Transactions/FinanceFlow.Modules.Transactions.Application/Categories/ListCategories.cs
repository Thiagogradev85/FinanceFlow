using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Categories;

public sealed record ListCategoriesQuery(Guid UserId) : IRequest<Result<IReadOnlyList<CategoryDto>>>;

public sealed class ListCategoriesHandler(ICategoryRepository categories)
    : IRequestHandler<ListCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(ListCategoriesQuery query, CancellationToken ct)
    {
        var items = await categories.ListByUserAsync(query.UserId, ct);
        IReadOnlyList<CategoryDto> dtos = items.Select(c => c.ToDto()).ToList();
        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }
}
