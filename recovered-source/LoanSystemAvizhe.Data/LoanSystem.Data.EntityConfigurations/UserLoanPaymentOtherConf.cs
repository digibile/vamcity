using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanPaymentOtherConf : IEntityTypeConfiguration<UserLoanPaymentOther>
{
    public void Configure(EntityTypeBuilder<UserLoanPaymentOther> builder)
    {
        builder.HasKey((UserLoanPaymentOther p) => p.Id);
        builder.Property((UserLoanPaymentOther x) => x.SavedBy).HasMaxLength(50);
        builder.Property((UserLoanPaymentOther x) => x.Description).HasMaxLength(1000);
        builder.HasOne((UserLoanPaymentOther x) => x.UserLoanPayment).WithMany((UserLoanPayment x) => x.UserLoanPaymentOthers).HasForeignKey((UserLoanPaymentOther x) => x.UserLoanPaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((UserLoanPaymentOther x) => x.MemberPurchase).WithMany().HasForeignKey((UserLoanPaymentOther x) => x.MemberPurchaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
