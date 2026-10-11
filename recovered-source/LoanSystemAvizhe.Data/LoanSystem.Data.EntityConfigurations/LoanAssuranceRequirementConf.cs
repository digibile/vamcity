using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanAssuranceRequirementConf : IEntityTypeConfiguration<LoanAssuranceRequirement>
{
	public void Configure(EntityTypeBuilder<LoanAssuranceRequirement> builder)
	{
		builder.HasKey((LoanAssuranceRequirement p) => p.Id);
		builder.HasOne((LoanAssuranceRequirement x) => x.Loan).WithMany((Loan x) => x.AssuranceRequirements).HasForeignKey((LoanAssuranceRequirement x) => x.LoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((LoanAssuranceRequirement x) => x.Customer).WithMany().HasForeignKey((LoanAssuranceRequirement x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
