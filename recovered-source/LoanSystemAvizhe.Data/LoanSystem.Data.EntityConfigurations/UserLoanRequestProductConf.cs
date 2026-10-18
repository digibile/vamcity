using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestProductConf : IEntityTypeConfiguration<UserLoanRequestProduct>
{
    public void Configure(EntityTypeBuilder<UserLoanRequestProduct> builder)
    {
        builder.HasKey((UserLoanRequestProduct p) => p.Id);
        builder.HasOne((UserLoanRequestProduct x) => x.Product).WithMany().HasForeignKey((UserLoanRequestProduct x) => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanRequestProduct x) => x.Customer).WithMany().HasForeignKey((UserLoanRequestProduct x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
