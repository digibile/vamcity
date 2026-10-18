using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class SiteContentConf : IEntityTypeConfiguration<SiteContent>
{
    public void Configure(EntityTypeBuilder<SiteContent> builder)
    {
        builder.HasKey((SiteContent p) => p.Id);
        builder.Property((SiteContent x) => x.Key).HasMaxLength(100);
        builder.HasOne((SiteContent x) => x.Customer).WithMany().HasForeignKey((SiteContent x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
