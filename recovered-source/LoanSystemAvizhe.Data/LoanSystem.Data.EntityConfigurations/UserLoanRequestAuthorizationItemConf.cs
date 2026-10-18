using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestAuthorizationItemConf : IEntityTypeConfiguration<UserLoanRequestAuthorizationItem>
{
    public void Configure(EntityTypeBuilder<UserLoanRequestAuthorizationItem> builder)
    {
        builder.HasKey((UserLoanRequestAuthorizationItem p) => p.Id);
        builder.Property((UserLoanRequestAuthorizationItem p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property((UserLoanRequestAuthorizationItem x) => x.MemberDescription).HasMaxLength(10000);
        builder.Property((UserLoanRequestAuthorizationItem x) => x.AdminDescription).HasMaxLength(1000);
        builder.Property((UserLoanRequestAuthorizationItem x) => x.Attachment).HasMaxLength(100);
        builder.HasOne((UserLoanRequestAuthorizationItem x) => x.UserLoanRequest).WithMany((UserLoanRequest x) => x.UserLoanRequestAuthorizationItems).HasForeignKey((UserLoanRequestAuthorizationItem x) => x.UserLoanRequestId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((UserLoanRequestAuthorizationItem x) => x.ThirdPersonAssuranceAuthorizationItem).WithOne((ThirdPersonAssuranceAuthorizationItem x) => x.UserLoanRequestAuthorizationItem).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((UserLoanRequestAuthorizationItem x) => x.EmzameContractAuthorizationItem).WithOne((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItem).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((UserLoanRequestAuthorizationItem x) => x.AuthorizationItem).WithMany().HasForeignKey((UserLoanRequestAuthorizationItem x) => x.AuthorizationItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
