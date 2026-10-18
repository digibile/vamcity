using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class MemberPurchaseConf : IEntityTypeConfiguration<MemberPurchase>
{
    public void Configure(EntityTypeBuilder<MemberPurchase> builder)
    {
        builder.HasKey((MemberPurchase p) => p.Id);
        builder.Property((MemberPurchase p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property((MemberPurchase x) => x.Description).HasMaxLength(200);
        builder.Property((MemberPurchase x) => x.SepJson).HasMaxLength(5000);
        builder.Property((MemberPurchase x) => x.ZarinPalJson).HasMaxLength(5000);
        builder.HasOne((MemberPurchase x) => x.UserLoanPayment).WithMany((UserLoanPayment x) => x.MemberPurchases).HasForeignKey((MemberPurchase x) => x.UserLoanPaymentId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((MemberPurchase x) => x.AgentFactor).WithMany().HasForeignKey((MemberPurchase x) => x.AgentFactorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
