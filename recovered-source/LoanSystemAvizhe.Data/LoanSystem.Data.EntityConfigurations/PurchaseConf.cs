using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class PurchaseConf : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey((Purchase p) => p.Id);
        builder.Property((Purchase x) => x.Description).HasMaxLength(200);
        builder.Property((Purchase x) => x.ZarinPalAuthority).HasMaxLength(50);
        builder.Property((Purchase x) => x.AffCode).HasMaxLength(20);
        builder.HasOne((Purchase x) => x.Customer).WithMany((Customer x) => x.Purchases).HasForeignKey((Purchase x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
