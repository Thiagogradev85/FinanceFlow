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

    public async Task<IReadOnlyList<Transaction>> ListRecentByUserAsync(
        Guid userId, int take, CancellationToken ct = default)
    {
        return await db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.OccurredOn)
            .ThenByDescending(t => t.CreatedAtUtc)
            .Take(take)
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

    public async Task<decimal> GetAllTimeNetAsync(Guid userId, DateOnly asOf, CancellationToken ct = default)
    {
        // Só o realizado: transações até a data de corte (asOf = hoje). Parcelas futuras ficam de fora.
        var realized = db.Transactions.Where(t => t.UserId == userId && t.OccurredOn <= asOf);

        var inflow = await realized
            .Where(t => t.Direction == TransactionDirection.Inflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var outflow = await realized
            .Where(t => t.Direction == TransactionDirection.Outflow)
            .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        return inflow - outflow;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetAllTimeNetByAccountsAsync(
        Guid userId, DateOnly asOf, CancellationToken ct = default)
    {
        // Uma query só, agrupada por conta — evita N+1 quando o chamador precisa do saldo
        // de todas as contas do usuário (ex.: tela de Contas).
        var rows = await db.Transactions
            .Where(t => t.UserId == userId && t.OccurredOn <= asOf)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Inflow = g.Where(t => t.Direction == TransactionDirection.Inflow).Sum(t => t.Amount),
                Outflow = g.Where(t => t.Direction == TransactionDirection.Outflow).Sum(t => t.Amount)
            })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.AccountId, r => r.Inflow - r.Outflow);
    }

    public async Task<IReadOnlyList<MonthCommitmentDto>> GetMonthlyCommittedInstallmentsAsync(
        Guid userId, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken ct = default)
    {
        // Parcelas (InstallmentGroupId != null) que vencem no intervalo. Agrupa por mês em memória
        // (volume baixo) para não depender da tradução de GroupBy(Year/Month) pro SQL.
        var rows = await db.Transactions
            .Where(t => t.UserId == userId
                && t.InstallmentGroupId != null
                && t.Direction == TransactionDirection.Outflow
                && t.OccurredOn >= fromInclusive && t.OccurredOn < toExclusive)
            .Select(t => new { t.OccurredOn, t.Amount })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => new { r.OccurredOn.Year, r.OccurredOn.Month })
            .Select(g => new MonthCommitmentDto(g.Key.Year, g.Key.Month, g.Sum(x => x.Amount)))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryBreakdownRawDto>> GetCategoryBreakdownAsync(
        Guid userId, int year, int month, CancellationToken ct = default)
    {
        var (start, end) = MonthRange(year, month);

        // JOIN explícito: TransactionConfiguration não declara navegação para Category,
        // então usamos join manual — mesmo SQL, sem precisar tocar no mapeamento.
        return await (
            from t in db.Transactions
            join c in db.Categories on t.CategoryId equals c.Id
            where t.UserId == userId
                  && t.Type != TransactionType.Transfer
                  && t.Direction == TransactionDirection.Outflow
                  && t.OccurredOn >= start && t.OccurredOn < end
            group new { t.Amount, c.Name, c.Color, c.Icon } by new { t.CategoryId, c.Name, c.Color, c.Icon } into g
            orderby g.Sum(x => x.Amount) descending
            select new CategoryBreakdownRawDto(
                g.Key.CategoryId!.Value,
                g.Key.Name,
                g.Key.Color,
                g.Key.Icon,
                g.Sum(x => x.Amount))
        ).ToListAsync(ct);
    }

    private static (DateOnly start, DateOnly end) MonthRange(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        return (start, start.AddMonths(1));
    }
}
