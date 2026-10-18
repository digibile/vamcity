using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestStatusHistoryConf : IEntityTypeConfiguration<UserLoanRequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<UserLoanRequestStatusHistory> builder)
    {
        builder.HasKey((UserLoanRequestStatusHistory p) => p.Id);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.User).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.UserLoanRequest).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.UserLoanRequestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.FromStatus).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.FromStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.FromAgentStatus).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.FromAgentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.ToStatus).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.ToStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanRequestStatusHistory x) => x.ToAgentStatus).WithMany().HasForeignKey((UserLoanRequestStatusHistory x) => x.ToAgentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
