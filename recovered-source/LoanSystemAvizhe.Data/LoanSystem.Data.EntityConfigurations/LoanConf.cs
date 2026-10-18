using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanConf : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey((Loan p) => p.Id);
        builder.Property((Loan x) => x.Title).HasMaxLength(100);
        builder.Property((Loan x) => x.FileName).HasMaxLength(100);
        builder.Property((Loan x) => x.PlanTitle).HasMaxLength(100);
        builder.Property((Loan x) => x.PlanShortDescription).HasMaxLength(1000);
        builder.Property((Loan x) => x.DisableText).HasMaxLength(100);
        builder.Property((Loan x) => x.PlanHtml).HasMaxLength(5000);
    }
}
