using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanRequestStatusConf : IEntityTypeConfiguration<LoanRequestStatus>
{
	public void Configure(EntityTypeBuilder<LoanRequestStatus> builder)
	{
		builder.HasKey((LoanRequestStatus p) => p.Id);
		builder.Property((LoanRequestStatus x) => x.Title).HasMaxLength(50).IsRequired();
		builder.HasOne((LoanRequestStatus x) => x.SmsPattern).WithMany().HasForeignKey((LoanRequestStatus x) => x.SmsPatternId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired(required: false);
		builder.HasOne((LoanRequestStatus x) => x.Customer).WithMany().HasForeignKey((LoanRequestStatus x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
