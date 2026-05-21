namespace FinanceFlow.Messaging.Kafka;

public sealed class KafkaSettings
{
    /// <summary>Endereço externo do broker (docker-compose expõe em localhost:29092).</summary>
    public string BootstrapServers { get; set; } = "localhost:29092";

    /// <summary>Prefixo dos topics — ex.: "financeflow.TransactionCreatedDomainEvent".</summary>
    public string TopicPrefix { get; set; } = "financeflow.";
}
