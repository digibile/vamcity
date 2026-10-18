using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ConsultantConf : IEntityTypeConfiguration<Consultant>
{
    public void Configure(EntityTypeBuilder<Consultant> builder)
    {
        builder.HasKey((Consultant p) => p.Id);
        builder.Property((Consultant x) => x.Description).HasMaxLength(1000);
        builder.Property((Consultant x) => x.CallResult).HasMaxLength(1000);
        builder.Property((Consultant x) => x.Name).HasMaxLength(100);
        builder.Property((Consultant x) => x.Phone).HasMaxLength(20);
    }
}
