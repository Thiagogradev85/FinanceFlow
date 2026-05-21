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
    Task<(decimal income, decimal expense)> GetMonthTotalsAsync(Guid userId, int year, int month, CancellationToken ct = default);
    Task<decimal> GetAllTimeNetAsync(Guid userId, CancellationToken ct = default);
}
