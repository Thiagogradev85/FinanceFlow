using FinanceFlow.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceFlow.Messaging.Kafka;

public static class KafkaServiceCollectionExtensions
{
    /// <summary>Fase 2: troca o LoggingEventBus por Kafka. Basta chamar isto no Program.cs.</summary>
    public static IServiceCollection AddKafkaEventBus(this IServiceCollection services, Action<KafkaSettings>? configure = null)
    {
        var settings = new KafkaSettings();
        configure?.Invoke(settings);

        services.AddSingleton(settings);
        services.AddSingleton<IEventBus, KafkaEventBus>();
        return services;
    }

    /// <summary>Fase 2: liga os consumers (projeções de saldo, auditoria, etc.).</summary>
    public static IServiceCollection AddKafkaConsumers(this IServiceCollection services)
    {
        services.AddHostedService<BalanceProjectionConsumer>();
        return services;
    }
}
