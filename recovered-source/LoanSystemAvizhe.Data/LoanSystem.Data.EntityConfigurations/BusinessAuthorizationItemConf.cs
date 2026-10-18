using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class BusinessAuthorizationItemConf : IEntityTypeConfiguration<BusinessAuthorizationItem>
{
    public void Configure(EntityTypeBuilder<BusinessAuthorizationItem> builder)
    {
        builder.HasKey((BusinessAuthorizationItem p) => p.Id);
        builder.HasOne((BusinessAuthorizationItem x) => x.AuthorizationItem).WithMany().HasForeignKey((BusinessAuthorizationItem x) => x.AuthorizationItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((BusinessAuthorizationItem x) => x.Customer).WithMany().HasForeignKey((BusinessAuthorizationItem x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
