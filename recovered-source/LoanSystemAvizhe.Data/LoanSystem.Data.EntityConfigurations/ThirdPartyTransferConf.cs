using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ThirdPartyTransferConf : IEntityTypeConfiguration<ThirdPartyTransfer>
{
    public void Configure(EntityTypeBuilder<ThirdPartyTransfer> builder)
    {
        builder.HasKey((ThirdPartyTransfer p) => p.Id);
        builder.Property((ThirdPartyTransfer x) => x.EntityType).HasMaxLength(50);
        builder.Property((ThirdPartyTransfer x) => x.EntityId).HasMaxLength(50);
        builder.Property((ThirdPartyTransfer x) => x.EntityTag).HasMaxLength(250);
        builder.HasOne((ThirdPartyTransfer x) => x.Customer).WithMany().HasForeignKey((ThirdPartyTransfer x) => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
