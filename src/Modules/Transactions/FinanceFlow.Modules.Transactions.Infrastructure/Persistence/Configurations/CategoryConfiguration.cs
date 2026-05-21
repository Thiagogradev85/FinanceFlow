using FinanceFlow.Modules.Transactions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceFlow.Modules.Transactions.Infrastructure;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("categories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(60).IsRequired();
        b.Property(x => x.Color).HasMaxLength(9);
        b.Property(x => x.Icon).HasMaxLength(40);
        b.Property(x => x.Kind).HasConversion<int>();

        b.HasIndex(x => x.UserId);

        // DomainEvents é comportamento, não dado — não vira coluna.
        b.Ignore(x => x.DomainEvents);
    }
}
