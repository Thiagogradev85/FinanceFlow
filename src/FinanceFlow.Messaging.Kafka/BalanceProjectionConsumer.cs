using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceFlow.Messaging.Kafka;

/// <summary>
/// SCAFFOLD da Fase 2 — ainda NÃO é registrado por padrão.
/// Consome TransactionCreatedDomainEvent e (futuramente) atualiza um saldo materializado por conta,
/// de forma idempotente. Aqui só loga, para você ver o consumer rodando antes de implementar a lógica.
/// </summary>
public sealed class BalanceProjectionConsumer(KafkaSettings settings, ILogger<BalanceProjectionConsumer> logger)
    : BackgroundService
{
    private const string GroupId = "financeflow.balance-projection";

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Rodamos o loop de consumo numa thread dedicada — Consume() é bloqueante.
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        var topic = settings.TopicPrefix + "TransactionCreatedDomainEvent";
        consumer.Subscribe(topic);
        logger.LogInformation("[KAFKA CONSUMER] inscrito em {Topic} (group {Group})", topic, GroupId);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = consumer.Consume(stoppingToken);
                if (message is null) continue;

                using var doc = JsonDocument.Parse(message.Message.Value);
                logger.LogInformation("[KAFKA CONSUMER] recebido offset {Offset}: {Payload}",
                    message.Offset.Value, message.Message.Value);

                // TODO Fase 2: atualizar saldo agregado da conta de forma idempotente (dedupe por EventId).
                consumer.Commit(message); // só commita o offset depois de processar com sucesso
            }
        }
        catch (OperationCanceledException)
        {
            // shutdown normal
        }
        finally
        {
            consumer.Close();
        }
    }
}
