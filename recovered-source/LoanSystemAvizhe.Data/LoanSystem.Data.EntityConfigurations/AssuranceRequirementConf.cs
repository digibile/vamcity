using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AssuranceRequirementConf : IEntityTypeConfiguration<AssuranceRequirement>
{
	public void Configure(EntityTypeBuilder<AssuranceRequirement> builder)
	{
		builder.HasKey((AssuranceRequirement p) => p.Id);
		builder.Property((AssuranceRequirement x) => x.Title).HasMaxLength(100);
		builder.HasOne((AssuranceRequirement x) => x.Customer).WithMany().HasForeignKey((AssuranceRequirement x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
