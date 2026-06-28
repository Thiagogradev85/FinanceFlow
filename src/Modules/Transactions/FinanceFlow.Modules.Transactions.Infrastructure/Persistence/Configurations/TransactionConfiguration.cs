using FinanceFlow.Modules.Transactions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.ToTable("transactions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.Description).HasMaxLength(280);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Direction).HasConversion<int>();

        b.HasIndex(x => new { x.UserId, x.OccurredOn });
        b.HasIndex(x => x.TransferGroupId);
        b.HasIndex(x => x.InstallmentGroupId);

        // Soft delete: transações apagadas somem das queries automaticamente.
        b.HasQueryFilter(x => !x.IsDeleted);
        b.Ignore(x => x.DomainEvents);
    }
}
