using FinanceFlow.Modules.Transactions.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

/// <summary>Registro de tudo que o módulo Transactions precisa no container DI.</summary>
public static class TransactionsModule
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TransactionsDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", TransactionsDbContext.Schema)));

        services.AddScoped<ITransactionsUnitOfWork>(sp => sp.GetRequiredService<TransactionsDbContext>());
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }
}
