using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AffiliateCommissionConf : IEntityTypeConfiguration<AffiliateCommission>
{
	public void Configure(EntityTypeBuilder<AffiliateCommission> builder)
	{
		builder.HasKey((AffiliateCommission p) => p.Id);
		builder.Property((AffiliateCommission x) => x.Description).HasMaxLength(500);
		builder.HasOne((AffiliateCommission x) => x.User).WithMany().HasForeignKey((AffiliateCommission x) => x.UserId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((AffiliateCommission x) => x.Purchase).WithMany((Purchase x) => x.AffiliateCommissions).HasForeignKey((AffiliateCommission x) => x.PurchaseId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
