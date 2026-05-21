using System.Text.Json;
using Confluent.Kafka;
using FinanceFlow.SharedKernel;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Messaging.Kafka;

/// <summary>
/// Implementação Kafka do IEventBus (entra na Fase 2). Serializa o evento em JSON e publica
/// num topic nomeado pelo tipo do evento. A Key garante ordenação por chave dentro da partição.
/// </summary>
public sealed class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly string _topicPrefix;

    public KafkaEventBus(KafkaSettings settings, ILogger<KafkaEventBus> logger)
    {
        _logger = logger;
        _topicPrefix = settings.TopicPrefix;
        var config = new ProducerConfig { BootstrapServers = settings.BootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        var topic = _topicPrefix + domainEvent.GetType().Name;
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        var result = await _producer.ProduceAsync(
            topic,
            new Message<string, string> { Key = domainEvent.EventId.ToString(), Value = payload },
            ct);

        _logger.LogInformation(
            "[KAFKA PRODUCER] topic={Topic} partition={Partition} offset={Offset}",
            topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose() => _producer.Dispose();
}
