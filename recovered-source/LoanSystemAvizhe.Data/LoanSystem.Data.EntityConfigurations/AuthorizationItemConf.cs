using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AuthorizationItemConf : IEntityTypeConfiguration<AuthorizationItem>
{
    public void Configure(EntityTypeBuilder<AuthorizationItem> builder)
    {
        builder.HasKey((AuthorizationItem p) => p.Id);
        builder.Property((AuthorizationItem x) => x.Title).HasMaxLength(100);
        builder.Property((AuthorizationItem x) => x.Description).HasMaxLength(10000);
        builder.HasOne((AuthorizationItem x) => x.Customer).WithMany().HasForeignKey((AuthorizationItem x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
