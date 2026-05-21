using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

internal sealed class CategoryRepository(TransactionsDbContext db) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Category entity, CancellationToken ct = default) =>
        await db.Categories.AddAsync(entity, ct);

    public void Update(Category entity) => db.Categories.Update(entity);

    public void Remove(Category entity) => db.Categories.Remove(entity);

    public async Task<IReadOnlyList<Category>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Categories
            .Where(c => c.UserId == userId && !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}
