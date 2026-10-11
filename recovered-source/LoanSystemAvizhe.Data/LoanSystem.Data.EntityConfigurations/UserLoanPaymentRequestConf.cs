using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanPaymentRequestConf : IEntityTypeConfiguration<UserLoanPaymentRequest>
{
	public void Configure(EntityTypeBuilder<UserLoanPaymentRequest> builder)
	{
		builder.HasKey((UserLoanPaymentRequest p) => p.Id);
		builder.Property((UserLoanPaymentRequest x) => x.TrackNumber).HasMaxLength(50);
		builder.Property((UserLoanPaymentRequest x) => x.FileName).HasMaxLength(50);
		builder.HasOne((UserLoanPaymentRequest p) => p.AdminAcceptor).WithMany().HasForeignKey((UserLoanPaymentRequest p) => p.AdminAcceptorId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserLoanPaymentRequest p) => p.UserLoanPayment).WithMany((UserLoanPayment p) => p.UserLoanPaymentRequests).HasForeignKey((UserLoanPaymentRequest p) => p.UserLoanPaymentId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
