using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AgentFactorConf : IEntityTypeConfiguration<AgentFactor>
{
	public void Configure(EntityTypeBuilder<AgentFactor> builder)
	{
		builder.HasKey((AgentFactor p) => p.Id);
		builder.Property((AgentFactor p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
		builder.Property((AgentFactor x) => x.UserCode).HasMaxLength(10);
		builder.Property((AgentFactor x) => x.Description).HasMaxLength(1000);
		builder.Property((AgentFactor x) => x.AdminDescription).HasMaxLength(1000);
		builder.HasOne((AgentFactor x) => x.MemberPurchase).WithMany().HasForeignKey((AgentFactor x) => x.MemberPurchaseId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((AgentFactor x) => x.Member).WithMany().HasForeignKey((AgentFactor x) => x.MemberId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((AgentFactor x) => x.Agent).WithMany().HasForeignKey((AgentFactor x) => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((AgentFactor x) => x.Customer).WithMany().HasForeignKey((AgentFactor x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
