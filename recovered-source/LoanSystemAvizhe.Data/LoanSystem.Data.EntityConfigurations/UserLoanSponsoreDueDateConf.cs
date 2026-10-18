using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanSponsoreDueDateConf : IEntityTypeConfiguration<UserLoanSponsoreDueDate>
{
    public void Configure(EntityTypeBuilder<UserLoanSponsoreDueDate> builder)
    {
        builder.HasKey((UserLoanSponsoreDueDate p) => p.Id);
        builder.Property((UserLoanSponsoreDueDate x) => x.Description).HasMaxLength(500);
        builder.HasOne((UserLoanSponsoreDueDate x) => x.UserLoan).WithMany().HasForeignKey((UserLoanSponsoreDueDate x) => x.UserLoanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((UserLoanSponsoreDueDate x) => x.Customer).WithMany().HasForeignKey((UserLoanSponsoreDueDate x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
