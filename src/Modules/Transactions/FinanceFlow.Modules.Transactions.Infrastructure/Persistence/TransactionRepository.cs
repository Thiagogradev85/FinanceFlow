using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

internal sealed class TransactionRepository(TransactionsDbContext db) : ITransactionRepository
{
    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Transaction entity, CancellationToken ct = default) =>
        await db.Transactions.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default) =>
        await db.Transactions.AddRangeAsync(transactions, ct);

    public void Update(Transaction entity) => db.Transactions.Update(entity);

    public void Remove(Transaction entity) => db.Transactions.Remove(entity);

    public async Task<IReadOnlyList<Transaction>> ListByUserAndMonthAsync(
        Guid userId, int year, int month, CancellationToken ct = default)
    {
        var (start, end) = MonthRange(year, month);
        return await db.Transactions
            .Where(t => t.UserId == userId && t.OccurredOn >= start && t.OccurredOn < end)
            .OrderByDescending(t => t.OccurredOn)
            .ThenByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<(decimal income, decimal expense)> GetMonthTotalsAsync(
        Guid userId, int year, int month, CancellationToken ct = default)
    {
        var (start, end) = MonthRange(year, month);

        // Transferências movem dinheiro entre contas próprias — não são receita nem despesa.
        var monthQuery = db.Transactions.Where(t =>
            t.UserId == userId &&
            t.Type != TransactionType.Transfer &&
            t.OccurredOn >= start && t.OccurredOn < end);

        var income = await monthQuery
            .Where(t => t.Direction == TransactionDirection.Inflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var expense = await monthQuery
            .Where(t => t.Direction == TransactionDirection.Outflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        return (income, expense);
    }

    public async Task<decimal> GetAllTimeNetAsync(Guid userId, CancellationToken ct = default)
    {
        var inflow = await db.Transactions
            .Where(t => t.UserId == userId && t.Direction == TransactionDirection.Inflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var outflow = await db.Transactions
            .Where(t => t.UserId == userId && t.Direction == TransactionDirection.Outflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        return inflow - outflow;
    }

    private static (DateOnly start, DateOnly end) MonthRange(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        return (start, start.AddMonths(1));
    }
}
