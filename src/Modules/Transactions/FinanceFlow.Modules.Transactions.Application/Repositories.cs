using FinanceFlow.Modules.Transactions.Domain;
using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Transactions.Application;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> ListByUserAsync(Guid userId, CancellationToken ct = default);
}

public interface ITransactionRepository : IRepository<Transaction>
{
    Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> ListByUserAndMonthAsync(Guid userId, int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> ListRecentByUserAsync(Guid userId, int take, CancellationToken ct = default);
    Task<(decimal income, decimal expense)> GetMonthTotalsAsync(Guid userId, int year, int month, CancellationToken ct = default);

    // Saldo "realizado": só conta o que já venceu (OccurredOn <= asOf). Parcelas futuras
    // ficam de fora do saldo atual — entram como "comprometido" nos meses seguintes.
    Task<decimal> GetAllTimeNetAsync(Guid userId, DateOnly asOf, CancellationToken ct = default);

    // Total parcelado comprometido por mês, no intervalo [fromInclusive, toExclusive).
    Task<IReadOnlyList<MonthCommitmentDto>> GetMonthlyCommittedInstallmentsAsync(
        Guid userId, DateOnly fromInclusive, DateOnly toExclusive, CancellationToken ct = default);
}
