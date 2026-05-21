namespace FinanceFlow.SharedKernel;

/// <summary>
/// Contrato de repositório por agregado. A Application só conhece esta interface —
/// nunca o DbContext (essa regra mantém o domínio independente do EF Core).
/// </summary>
public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
