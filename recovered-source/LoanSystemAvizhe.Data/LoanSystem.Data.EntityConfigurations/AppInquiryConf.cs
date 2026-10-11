using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AppInquiryConf : IEntityTypeConfiguration<AppInquiry>
{
	public void Configure(EntityTypeBuilder<AppInquiry> builder)
	{
		builder.HasKey((AppInquiry p) => p.Id);
		builder.Property((AppInquiry x) => x.Key).HasMaxLength(100);
		builder.Property((AppInquiry x) => x.Tag).HasMaxLength(100);
		builder.HasOne((AppInquiry x) => x.Customer).WithMany().HasForeignKey((AppInquiry x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
