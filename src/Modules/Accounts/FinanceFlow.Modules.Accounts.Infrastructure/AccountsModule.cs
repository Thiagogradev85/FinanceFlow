using FinanceFlow.Modules.Accounts.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceFlow.Modules.Accounts.Infrastructure;

public static class AccountsModule
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AccountsDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", AccountsDbContext.Schema)));

        services.AddScoped<IAccountsUnitOfWork>(sp => sp.GetRequiredService<AccountsDbContext>());
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
