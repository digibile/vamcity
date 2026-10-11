using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AgentLoanAuthorizationItemConf : IEntityTypeConfiguration<AgentLoanAuthorizationItem>
{
	public void Configure(EntityTypeBuilder<AgentLoanAuthorizationItem> builder)
	{
		builder.HasKey((AgentLoanAuthorizationItem p) => p.Id);
		builder.HasOne((AgentLoanAuthorizationItem x) => x.AuthorizationItem).WithMany().HasForeignKey((AgentLoanAuthorizationItem x) => x.AuthorizationItemId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((AgentLoanAuthorizationItem x) => x.Loan).WithMany((Loan x) => x.AgentLoanAuthorizationItems).HasForeignKey((AgentLoanAuthorizationItem x) => x.LoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((AgentLoanAuthorizationItem x) => x.Customer).WithMany().HasForeignKey((AgentLoanAuthorizationItem x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
