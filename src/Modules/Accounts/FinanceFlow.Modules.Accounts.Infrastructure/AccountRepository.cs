using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Accounts.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Modules.Accounts.Infrastructure;

internal sealed class AccountRepository(AccountsDbContext db) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(Account entity, CancellationToken ct = default) =>
        await db.Accounts.AddAsync(entity, ct);

    public void Update(Account entity) => db.Accounts.Update(entity);

    public void Remove(Account entity) => db.Accounts.Remove(entity);

    public async Task<IReadOnlyList<Account>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Accounts
            .Where(a => a.UserId == userId && !a.IsArchived)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.Name)
            .ToListAsync(ct);

    public async Task<decimal> GetOpeningTotalAsync(Guid userId, CancellationToken ct = default) =>
        await db.Accounts
            .Where(a => a.UserId == userId && !a.IsArchived)
            .SumAsync(a => (decimal?)a.OpeningBalance, ct) ?? 0m;
}
