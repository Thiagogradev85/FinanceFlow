namespace FinanceFlow.SharedKernel;

/// <summary>
/// Publica eventos de domínio para fora do módulo. A abstração mora aqui (SharedKernel)
/// para que o domínio dependa do contrato, não do Kafka. A implementação concreta
/// (Confluent.Kafka) vive em FinanceFlow.Messaging.Kafka e entra de verdade na Fase 2.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent;
}
