using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestConf : IEntityTypeConfiguration<UserLoanRequest>
{
	public void Configure(EntityTypeBuilder<UserLoanRequest> builder)
	{
		builder.HasKey((UserLoanRequest p) => p.Id);
		builder.Property((UserLoanRequest p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
		builder.Property((UserLoanRequest x) => x.Comment).HasMaxLength(1000);
		builder.Property((UserLoanRequest x) => x.LastSponsorComment).HasMaxLength(1000);
		builder.Property((UserLoanRequest x) => x.AdminDescriptionPrivate).HasMaxLength(1000);
		builder.HasOne((UserLoanRequest x) => x.User).WithMany().HasForeignKey((UserLoanRequest x) => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((UserLoanRequest x) => x.Loan).WithMany().HasForeignKey((UserLoanRequest x) => x.LoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((UserLoanRequest x) => x.Agent).WithMany().HasForeignKey((UserLoanRequest x) => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserLoanRequest x) => x.Sponsor).WithMany().HasForeignKey((UserLoanRequest x) => x.SponsorId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserLoanRequest x) => x.Status).WithMany().HasForeignKey((UserLoanRequest x) => x.StatusId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserLoanRequest x) => x.AgentStatus).WithMany().HasForeignKey((UserLoanRequest x) => x.AgentStatusId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
