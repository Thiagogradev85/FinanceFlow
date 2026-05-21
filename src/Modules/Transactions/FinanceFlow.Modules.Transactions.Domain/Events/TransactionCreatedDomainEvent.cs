using FinanceFlow.SharedKernel;

namespace FinanceFlow.Modules.Transactions.Domain;

/// <summary>
/// Disparado quando uma transação é criada. Na Fase 1 é publicado num IEventBus que só loga.
/// Na Fase 2 o IEventBus vira Kafka e um consumer atualiza o saldo agregado da conta de forma
/// assíncrona e idempotente. O domínio não muda — só troca a implementação do barramento.
/// </summary>
public sealed record TransactionCreatedDomainEvent(
    Guid TransactionId,
    Guid UserId,
    Guid AccountId,
    TransactionDirection Direction,
    decimal Amount,
    string Currency,
    DateOnly OccurredOn) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
