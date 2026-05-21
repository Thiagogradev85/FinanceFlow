using FinanceFlow.SharedKernel;

namespace FinanceFlow.Api.Common;

/// <summary>
/// IEventBus padrão da Fase 1: só registra o evento no log. O fluxo de eventos já existe
/// (entidade → Raise → handler publica), só o TRANSPORTE é simples. Na Fase 2 troca-se
/// este registro por AddKafkaEventBus() e nada mais no domínio muda.
/// </summary>
public sealed class LoggingEventBus(ILogger<LoggingEventBus> logger) : IEventBus
{
    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        logger.LogInformation("[EVENT] {EventType} {@Event}", domainEvent.GetType().Name, domainEvent);
        return Task.CompletedTask;
    }
}
