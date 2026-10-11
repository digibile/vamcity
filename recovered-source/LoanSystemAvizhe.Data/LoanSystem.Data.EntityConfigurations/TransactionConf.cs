using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class TransactionConf : IEntityTypeConfiguration<Transaction>
{
	public void Configure(EntityTypeBuilder<Transaction> builder)
	{
		builder.Property((Transaction x) => x.FileName).HasMaxLength(50);
		builder.Property((Transaction x) => x.FileName2).HasMaxLength(50);
		builder.Property((Transaction x) => x.FileName3).HasMaxLength(50);
		builder.HasOne((Transaction x) => x.SponsoredLoan).WithMany().HasForeignKey((Transaction x) => x.SponsoredLoanId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.UserLoan).WithMany().HasForeignKey((Transaction x) => x.UserLoanId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.UserLoanContact).WithMany().HasForeignKey((Transaction x) => x.UserLoanContactId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.CustomerMarketerCommission).WithMany().HasForeignKey((Transaction x) => x.CustomerMarketerCommissionId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.Customer).WithMany().HasForeignKey((Transaction x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.Agent).WithMany().HasForeignKey((Transaction x) => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((Transaction x) => x.Contact).WithMany().HasForeignKey((Transaction x) => x.ContactId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
	}
}
