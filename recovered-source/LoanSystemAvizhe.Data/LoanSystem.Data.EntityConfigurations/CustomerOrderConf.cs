using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class CustomerOrderConf : IEntityTypeConfiguration<CustomerOrder>
{
	public void Configure(EntityTypeBuilder<CustomerOrder> builder)
	{
		builder.HasKey((CustomerOrder p) => p.Id);
		builder.Property((CustomerOrder x) => x.Description).HasMaxLength(1000);
		builder.Property((CustomerOrder x) => x.CallResult).HasMaxLength(1000);
		builder.Property((CustomerOrder x) => x.Name).HasMaxLength(100);
		builder.Property((CustomerOrder x) => x.Phone).HasMaxLength(20);
		builder.Property((CustomerOrder x) => x.NationalCode).HasMaxLength(20);
		builder.Property((CustomerOrder x) => x.ProductName).HasMaxLength(200);
		builder.HasOne((CustomerOrder x) => x.Customer).WithMany().HasForeignKey((CustomerOrder x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
