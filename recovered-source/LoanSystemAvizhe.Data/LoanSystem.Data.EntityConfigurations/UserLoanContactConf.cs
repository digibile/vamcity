using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanContactConf : IEntityTypeConfiguration<UserLoanContact>
{
	public void Configure(EntityTypeBuilder<UserLoanContact> builder)
	{
		builder.HasKey((UserLoanContact p) => p.Id);
		builder.HasOne((UserLoanContact x) => x.Contact).WithMany((Contact x) => x.UserLoanContacts).HasForeignKey((UserLoanContact x) => x.ContactId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserLoanContact x) => x.UserLoan).WithMany().HasForeignKey((UserLoanContact x) => x.UserLoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((UserLoanContact x) => x.Customer).WithMany().HasForeignKey((UserLoanContact x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
