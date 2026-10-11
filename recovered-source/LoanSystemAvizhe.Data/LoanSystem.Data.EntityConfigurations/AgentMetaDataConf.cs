using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AgentMetaDataConf : IEntityTypeConfiguration<AgentMetaData>
{
	public void Configure(EntityTypeBuilder<AgentMetaData> builder)
	{
		builder.HasKey((AgentMetaData p) => p.Id);
		builder.Property((AgentMetaData x) => x.SiteUrl).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.ShenaseMelli).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.ShomareSabt).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.AgentCategoryActivities).HasMaxLength(200);
		builder.Property((AgentMetaData x) => x.AsasnameFileName).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.CheckedBy).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.CompanyName).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.EjarenameFileName).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.KartMelliFileName).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.RoznameRasmiFileName).HasMaxLength(100);
		builder.Property((AgentMetaData x) => x.SignaturesOwnerNames).HasMaxLength(200);
		builder.HasOne((AgentMetaData x) => x.User).WithOne((User x) => x.AgentMetaData).HasForeignKey((User x) => x.AgentMetaDataId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((AgentMetaData x) => x.Customer).WithMany().HasForeignKey((AgentMetaData x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
