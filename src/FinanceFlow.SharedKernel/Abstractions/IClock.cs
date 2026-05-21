namespace FinanceFlow.SharedKernel;

/// <summary>
/// Fonte de tempo injetável. Regra do projeto: nunca usar DateTime.Now direto —
/// sempre IClock (ou DateTime.UtcNow). Assim dá para "congelar" o tempo nos testes.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
