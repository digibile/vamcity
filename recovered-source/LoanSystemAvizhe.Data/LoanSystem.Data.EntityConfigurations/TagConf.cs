using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class TagConf : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey((Tag p) => p.Id);
        builder.Property((Tag x) => x.Title).HasMaxLength(100);
        builder.HasOne((Tag x) => x.Customer).WithMany().HasForeignKey((Tag x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
