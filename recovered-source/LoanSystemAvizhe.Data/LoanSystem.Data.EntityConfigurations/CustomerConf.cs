using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class CustomerConf : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property((Customer p) => p.Code).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property((Customer x) => x.AffCode).HasMaxLength(20);
        builder.Property((Customer x) => x.ParkDomain).HasMaxLength(100);
    }
}
