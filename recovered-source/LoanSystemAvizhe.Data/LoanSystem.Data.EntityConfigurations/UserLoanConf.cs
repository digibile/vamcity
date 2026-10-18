using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanConf : IEntityTypeConfiguration<UserLoan>
{
    public void Configure(EntityTypeBuilder<UserLoan> builder)
    {
        builder.Property((UserLoan p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property((UserLoan x) => x.Status).HasMaxLength(50);
        builder.Property((UserLoan x) => x.SecondaryCode).HasMaxLength(50);
        builder.HasOne((UserLoan x) => x.UserLoanRequest).WithMany().HasForeignKey((UserLoan x) => x.UserLoanRequestId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(required: false);
        builder.HasOne((UserLoan x) => x.Loan).WithMany().HasForeignKey((UserLoan x) => x.LoanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoan x) => x.Agent).WithMany().HasForeignKey((UserLoan x) => x.AgentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(required: false);
        builder.Property((UserLoan p) => p.MonthGap).HasDefaultValue(1);
    }
}
