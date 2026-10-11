using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AssuranceRequirementItemConf : IEntityTypeConfiguration<AssuranceRequirementItem>
{
	public void Configure(EntityTypeBuilder<AssuranceRequirementItem> builder)
	{
		builder.HasKey((AssuranceRequirementItem p) => p.Id);
		builder.Property((AssuranceRequirementItem x) => x.Title).HasMaxLength(100);
		builder.HasOne((AssuranceRequirementItem x) => x.AssuranceRequirement).WithMany((AssuranceRequirement x) => x.AssuranceRequirementItems).HasForeignKey((AssuranceRequirementItem x) => x.AssuranceRequirementId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((AssuranceRequirementItem x) => x.Customer).WithMany().HasForeignKey((AssuranceRequirementItem x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
