using FinanceFlow.SharedKernel;
using MediatR;

namespace FinanceFlow.Modules.Transactions.Application.Transactions;

public sealed record CategoryBreakdownItemDto(
    Guid CategoryId,
    string CategoryName,
    string Color,
    string Icon,
    decimal Total,
    decimal PercentOfExpense);

public sealed record GetCategoryBreakdownQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<IReadOnlyList<CategoryBreakdownItemDto>>>;

public sealed class GetCategoryBreakdownHandler(ITransactionRepository transactions)
    : IRequestHandler<GetCategoryBreakdownQuery, Result<IReadOnlyList<CategoryBreakdownItemDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryBreakdownItemDto>>> Handle(
        GetCategoryBreakdownQuery query, CancellationToken ct)
    {
        var rows = await transactions.GetCategoryBreakdownAsync(query.UserId, query.Year, query.Month, ct);

        // Percentual calculado aqui, não no banco: evita divisão por zero e lógica condicional em SQL.
        var totalExpense = rows.Sum(r => r.Total);

        IReadOnlyList<CategoryBreakdownItemDto> items = rows
            .Select(r => new CategoryBreakdownItemDto(
                r.CategoryId,
                r.CategoryName,
                r.Color,
                r.Icon,
                r.Total,
                totalExpense == 0 ? 0m : Math.Round(r.Total / totalExpense * 100, 1)))
            .ToList();

        return Result<IReadOnlyList<CategoryBreakdownItemDto>>.Success(items);
    }
}
