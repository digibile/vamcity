using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class SubAgentTransactionConf : IEntityTypeConfiguration<SubAgentTransaction>
{
	public void Configure(EntityTypeBuilder<SubAgentTransaction> builder)
	{
		builder.Property((SubAgentTransaction x) => x.FileName).HasMaxLength(50);
		builder.Property((SubAgentTransaction x) => x.SavedBy).HasMaxLength(100);
		builder.Property((SubAgentTransaction x) => x.Description).HasMaxLength(500);
		builder.HasOne((SubAgentTransaction x) => x.UserLoan).WithMany().HasForeignKey((SubAgentTransaction x) => x.UserLoanId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((SubAgentTransaction x) => x.Customer).WithMany().HasForeignKey((SubAgentTransaction x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
	}
}
