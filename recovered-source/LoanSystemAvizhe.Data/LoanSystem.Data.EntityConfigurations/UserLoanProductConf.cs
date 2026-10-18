using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanProductConf : IEntityTypeConfiguration<UserLoanProduct>
{
    public void Configure(EntityTypeBuilder<UserLoanProduct> builder)
    {
        builder.HasKey((UserLoanProduct p) => p.Id);
        builder.HasOne((UserLoanProduct x) => x.Product).WithMany().HasForeignKey((UserLoanProduct x) => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((UserLoanProduct x) => x.Customer).WithMany().HasForeignKey((UserLoanProduct x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
