using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class SmsPatternConf : IEntityTypeConfiguration<SmsPattern>
{
	public void Configure(EntityTypeBuilder<SmsPattern> builder)
	{
		builder.HasKey((SmsPattern p) => p.Id);
		builder.Property((SmsPattern x) => x.Title).HasMaxLength(200);
		builder.Property((SmsPattern x) => x.PatternBaseCode).HasMaxLength(20);
		builder.HasOne((SmsPattern x) => x.Customer).WithMany().HasForeignKey((SmsPattern x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
