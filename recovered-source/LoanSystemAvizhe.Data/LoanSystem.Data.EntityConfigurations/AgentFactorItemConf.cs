using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AgentFactorItemConf : IEntityTypeConfiguration<AgentFactorItem>
{
    public void Configure(EntityTypeBuilder<AgentFactorItem> builder)
    {
        builder.HasKey((AgentFactorItem p) => p.Id);
        builder.Property((AgentFactorItem x) => x.Product).HasMaxLength(250);
        builder.HasOne((AgentFactorItem x) => x.AgentFactor).WithMany((AgentFactor x) => x.AgentFactorItems).HasForeignKey((AgentFactorItem x) => x.AgentFactorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((AgentFactorItem x) => x.Customer).WithMany().HasForeignKey((AgentFactorItem x) => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
