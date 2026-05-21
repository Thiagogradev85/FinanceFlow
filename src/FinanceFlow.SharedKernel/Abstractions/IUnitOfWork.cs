namespace FinanceFlow.SharedKernel;

/// <summary>
/// Confirma todas as mudanças pendentes numa única transação.
/// É aqui que a transferência (2 pernas) commita atômica: tudo ou nada.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
