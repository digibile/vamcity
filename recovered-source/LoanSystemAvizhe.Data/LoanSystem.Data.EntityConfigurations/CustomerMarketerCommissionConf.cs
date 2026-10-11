using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class CustomerMarketerCommissionConf : IEntityTypeConfiguration<CustomerMarketerCommission>
{
	public void Configure(EntityTypeBuilder<CustomerMarketerCommission> builder)
	{
		builder.HasKey((CustomerMarketerCommission p) => p.Id);
		builder.Property((CustomerMarketerCommission x) => x.Description).HasMaxLength(500);
		builder.HasOne((CustomerMarketerCommission x) => x.Marketer).WithMany().HasForeignKey((CustomerMarketerCommission x) => x.MarketerId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((CustomerMarketerCommission x) => x.UserLoan).WithMany().HasForeignKey((CustomerMarketerCommission x) => x.UserLoanId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
