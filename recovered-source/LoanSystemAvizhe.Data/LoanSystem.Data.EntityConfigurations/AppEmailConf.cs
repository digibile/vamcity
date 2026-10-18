using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AppEmailConf : IEntityTypeConfiguration<AppEmail>
{
    public void Configure(EntityTypeBuilder<AppEmail> builder)
    {
        builder.HasKey((AppEmail p) => p.Id);
        builder.Property((AppEmail x) => x.To).HasMaxLength(50);
        builder.Property((AppEmail x) => x.EntityType).HasMaxLength(50);
        builder.Property((AppEmail x) => x.PatternTitle).HasMaxLength(100);
        builder.Property((AppEmail x) => x.Tag).HasMaxLength(100);
        builder.HasOne((AppEmail x) => x.Customer).WithMany().HasForeignKey((AppEmail x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
