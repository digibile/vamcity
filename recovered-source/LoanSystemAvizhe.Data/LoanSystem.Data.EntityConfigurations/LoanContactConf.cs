using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanContactConf : IEntityTypeConfiguration<LoanContact>
{
	public void Configure(EntityTypeBuilder<LoanContact> builder)
	{
		builder.HasKey((LoanContact p) => p.Id);
		builder.HasOne((LoanContact x) => x.Contact).WithMany().HasForeignKey((LoanContact x) => x.ContactId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((LoanContact x) => x.Loan).WithMany((Loan x) => x.LoanContacts).HasForeignKey((LoanContact x) => x.LoanId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((LoanContact x) => x.Customer).WithMany().HasForeignKey((LoanContact x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
