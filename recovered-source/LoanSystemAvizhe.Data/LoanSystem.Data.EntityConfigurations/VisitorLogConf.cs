using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class VisitorLogConf : IEntityTypeConfiguration<VisitorLog>
{
	public void Configure(EntityTypeBuilder<VisitorLog> builder)
	{
		builder.HasKey((VisitorLog p) => p.Id);
		builder.Property((VisitorLog x) => x.IpAddress).HasMaxLength(50);
		builder.Property((VisitorLog x) => x.UserAgent).HasMaxLength(200);
	}
}
