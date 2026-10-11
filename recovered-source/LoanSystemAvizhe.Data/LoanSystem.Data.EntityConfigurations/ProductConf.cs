using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ProductConf : IEntityTypeConfiguration<Product>
{
	public void Configure(EntityTypeBuilder<Product> builder)
	{
		builder.HasKey((Product p) => p.Id);
		builder.Property((Product x) => x.Name).HasMaxLength(200);
		builder.Property((Product x) => x.Code).HasMaxLength(10);
		builder.HasOne((Product x) => x.Customer).WithMany().HasForeignKey((Product x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
