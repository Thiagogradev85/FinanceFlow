using FinanceFlow.Modules.Transactions.Application;
using FinanceFlow.Modules.Transactions.Domain;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

/// <summary>
/// O DbContext do módulo. Implementa ITransactionsUnitOfWork: salvar = SaveChangesAsync daqui.
/// Cada módulo tem seu próprio schema no Postgres ("transactions") — isolamento de monolito modular.
/// </summary>
public sealed class TransactionsDbContext(DbContextOptions<TransactionsDbContext> options)
    : DbContext(options), ITransactionsUnitOfWork
{
    public const string Schema = "transactions";

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransactionsDbContext).Assembly);
    }
}
