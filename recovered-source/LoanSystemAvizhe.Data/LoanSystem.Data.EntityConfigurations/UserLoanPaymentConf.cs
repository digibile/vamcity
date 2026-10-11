using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanPaymentConf : IEntityTypeConfiguration<UserLoanPayment>
{
	public void Configure(EntityTypeBuilder<UserLoanPayment> builder)
	{
		builder.Property((UserLoanPayment p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
		builder.HasOne((UserLoanPayment x) => x.UserLoan).WithMany((UserLoan x) => x.UserLoanPayments).HasForeignKey((UserLoanPayment x) => x.UserLoanId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
