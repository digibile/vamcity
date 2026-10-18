using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AppSmsConf : IEntityTypeConfiguration<AppSms>
{
    public void Configure(EntityTypeBuilder<AppSms> builder)
    {
        builder.HasKey((AppSms p) => p.Id);
        builder.Property((AppSms x) => x.Text).HasMaxLength(1500);
        builder.Property((AppSms x) => x.To).HasMaxLength(50);
        builder.Property((AppSms x) => x.EntityType).HasMaxLength(50);
        builder.Property((AppSms x) => x.SmsPatternTitle).HasMaxLength(100);
        builder.Property((AppSms x) => x.Tag).HasMaxLength(100);
        builder.HasOne((AppSms x) => x.Customer).WithMany().HasForeignKey((AppSms x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
