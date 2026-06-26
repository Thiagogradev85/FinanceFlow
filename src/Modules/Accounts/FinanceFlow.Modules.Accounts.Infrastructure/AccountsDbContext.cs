using FinanceFlow.Modules.Accounts.Application;
using FinanceFlow.Modules.Accounts.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceFlow.Modules.Accounts.Infrastructure;

public sealed class AccountsDbContext(DbContextOptions<AccountsDbContext> options)
    : DbContext(options), IAccountsUnitOfWork
{
    public const string Schema = "accounts";

    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountsDbContext).Assembly);
    }
}

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("accounts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(80).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.IsPrimary).HasDefaultValue(false);

        b.HasIndex(x => x.UserId);
        b.Ignore(x => x.DomainEvents);
    }
}
