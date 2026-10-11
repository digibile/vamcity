using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ContactConf : IEntityTypeConfiguration<Contact>
{
	public void Configure(EntityTypeBuilder<Contact> builder)
	{
		builder.HasKey((Contact p) => p.Id);
		builder.Property((Contact x) => x.BusinessName).HasMaxLength(100);
		builder.Property((Contact x) => x.FullName).HasMaxLength(100);
		builder.Property((Contact x) => x.Address).HasMaxLength(250);
		builder.Property((Contact x) => x.Phone).HasMaxLength(50);
		builder.HasOne((Contact x) => x.Customer).WithMany().HasForeignKey((Contact x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
