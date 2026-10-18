using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ThirdPersonAssuranceAuthorizationItemConf : IEntityTypeConfiguration<ThirdPersonAssuranceAuthorizationItem>
{
    public void Configure(EntityTypeBuilder<ThirdPersonAssuranceAuthorizationItem> builder)
    {
        builder.HasKey((ThirdPersonAssuranceAuthorizationItem p) => p.Id);
        builder.Property((ThirdPersonAssuranceAuthorizationItem p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.HasOne((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItem).WithOne((UserLoanRequestAuthorizationItem x) => x.ThirdPersonAssuranceAuthorizationItem).HasForeignKey((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItemId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((ThirdPersonAssuranceAuthorizationItem x) => x.User).WithMany().HasForeignKey((ThirdPersonAssuranceAuthorizationItem x) => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
