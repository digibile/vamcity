using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class OffDateConf : IEntityTypeConfiguration<OffDate>
{
    public void Configure(EntityTypeBuilder<OffDate> builder)
    {
        builder.HasKey((OffDate p) => p.Id);
        builder.Property((OffDate x) => x.Title).HasMaxLength(100);
        builder.HasOne((OffDate x) => x.Customer).WithMany().HasForeignKey((OffDate x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
