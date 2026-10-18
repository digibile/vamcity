using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestSponsorHistoryConf : IEntityTypeConfiguration<UserLoanRequestSponsorHistory>
{
    public void Configure(EntityTypeBuilder<UserLoanRequestSponsorHistory> builder)
    {
        builder.HasKey((UserLoanRequestSponsorHistory p) => p.Id);
        builder.Property((UserLoanRequestSponsorHistory x) => x.Subject).HasMaxLength(100);
        builder.Property((UserLoanRequestSponsorHistory x) => x.Description).HasMaxLength(1000);
        builder.HasOne((UserLoanRequestSponsorHistory x) => x.User).WithMany().HasForeignKey((UserLoanRequestSponsorHistory x) => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((UserLoanRequestSponsorHistory x) => x.UserLoanRequest).WithMany().HasForeignKey((UserLoanRequestSponsorHistory x) => x.UserLoanRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
