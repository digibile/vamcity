using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class LoanTagConf : IEntityTypeConfiguration<LoanTag>
{
    public void Configure(EntityTypeBuilder<LoanTag> builder)
    {
        builder.HasKey((LoanTag p) => new { p.LoanId, p.TagId });
        builder.HasOne((LoanTag x) => x.Loan).WithMany((Loan x) => x.LoanTags).HasForeignKey((LoanTag x) => x.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((LoanTag x) => x.Tag).WithMany((Tag x) => x.LoanTags).HasForeignKey((LoanTag x) => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
