using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanAuthorizationItemConf : IEntityTypeConfiguration<LoanAuthorizationItem>
{
	public void Configure(EntityTypeBuilder<LoanAuthorizationItem> builder)
	{
		builder.HasKey((LoanAuthorizationItem p) => p.Id);
		builder.HasOne((LoanAuthorizationItem x) => x.AuthorizationItem).WithMany().HasForeignKey((LoanAuthorizationItem x) => x.AuthorizationItemId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((LoanAuthorizationItem x) => x.Loan).WithMany((Loan x) => x.LoanAuthorizationItems).HasForeignKey((LoanAuthorizationItem x) => x.LoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((LoanAuthorizationItem x) => x.Customer).WithMany().HasForeignKey((LoanAuthorizationItem x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
