using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ReliabilityConf : IEntityTypeConfiguration<Reliability>
{
	public void Configure(EntityTypeBuilder<Reliability> builder)
	{
		builder.HasKey((Reliability p) => p.Id);
		builder.Property((Reliability x) => x.Title).HasMaxLength(50);
		builder.Property((Reliability x) => x.Color).HasMaxLength(10);
		builder.HasOne((Reliability x) => x.Customer).WithMany().HasForeignKey((Reliability x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
