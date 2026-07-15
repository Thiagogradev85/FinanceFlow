using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Domain;

namespace FinanceFlow.UnitTests;

// Repositório em memória só para testar handlers isolados do EF Core (sem banco).
// Implementa de verdade só o que os testes usam; o resto não é exercitado aqui.
public sealed class FakeTransactionRepository(IEnumerable<Transaction> transactions) : ITransactionRepository
{
    private readonly List<Transaction> _transactions = transactions.ToList();

    public Task<IReadOnlyDictionary<Guid, decimal>> GetAllTimeNetByAccountsAsync(Guid userId, DateOnly asOf, CancellationToken ct = default)
    {
        // !IsDeleted é filtrado aqui manualmente; no TransactionRepository real esse filtro vem
        // de um HasQueryFilter global do EF (TransactionConfiguration), não de um Where explícito.
        // Replicamos o efeito, não o mecanismo.
        var nets = _transactions
            .Where(t => t.UserId == userId && !t.IsDeleted && t.OccurredOn <= asOf)
            .GroupBy(t => t.AccountId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(t => t.Direction == TransactionDirection.Inflow ? t.Amount : -t.Amount));

        return Task.FromResult<IReadOnlyDictionary<Guid, decimal>>(nets);
    }

    public Task<decimal> GetAllTimeNetAsync(Guid userId, DateOnly asOf, CancellationToken ct = default)
    {
        var realized = _transactions.Where(t => t.UserId == userId && !t.IsDeleted && t.OccurredOn <= asOf);
        var inflow = realized.Where(t => t.Direction == TransactionDirection.Inflow).Sum(t => t.Amount);
        var outflow = realized.Where(t => t.Direction == TransactionDirection.Outflow).Sum(t => t.Amount);
        return Task.FromResult(inflow - outflow);
    }

    public Task<(decimal income, decimal expense)> GetMonthTotalsAsync(Guid userId, int year, int month, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<MonthCommitmentDto>> GetMonthlyCommittedInstallmentsAsync(Guid userId, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<CategoryBreakdownRawDto>> GetCategoryBreakdownAsync(Guid userId, int year, int month, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<Transaction>> ListByUserAndMonthAsync(Guid userId, int year, int month, CancellationToken ct = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1);
        IReadOnlyList<Transaction> result = _transactions
            .Where(t => t.UserId == userId && !t.IsDeleted && t.OccurredOn >= start && t.OccurredOn < end)
            .OrderByDescending(t => t.OccurredOn)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Transaction>> ListRecentByUserAsync(Guid userId, int take, CancellationToken ct = default)
    {
        IReadOnlyList<Transaction> result = _transactions
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.OccurredOn)
            .Take(take)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task AddAsync(Transaction entity, CancellationToken ct = default)
        => throw new NotImplementedException();

    public void Update(Transaction entity)
        => throw new NotImplementedException();

    public void Remove(Transaction entity)
        => throw new NotImplementedException();
}
