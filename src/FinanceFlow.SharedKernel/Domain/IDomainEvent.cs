namespace FinanceFlow.SharedKernel;

/// <summary>
/// Marca um evento de domínio — algo relevante que aconteceu (ex.: TransactionCreated).
/// Na Fase 2 esses eventos viram mensagens Kafka via <see cref="IEventBus"/>.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}
