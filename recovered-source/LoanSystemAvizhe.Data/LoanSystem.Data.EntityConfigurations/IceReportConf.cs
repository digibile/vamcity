using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class IceReportConf : IEntityTypeConfiguration<IceReport>
{
    public void Configure(EntityTypeBuilder<IceReport> builder)
    {
        builder.HasKey((IceReport p) => p.Id);
        builder.Property((IceReport x) => x.Result).HasMaxLength(20);
        builder.Property((IceReport x) => x.Url).HasMaxLength(500);
        builder.Property((IceReport x) => x.Status).HasMaxLength(100);
        builder.Property((IceReport x) => x.AttachmentFileName).HasMaxLength(200);
        builder.Property((IceReport x) => x.ReportPdfFileName).HasMaxLength(200);
        builder.Property((IceReport x) => x.ReportID).HasMaxLength(50);
        builder.Property((IceReport x) => x.IceReportJson).HasMaxLength(1000);
        builder.Property((IceReport x) => x.Errors).HasMaxLength(200);
        builder.HasOne((IceReport x) => x.Customer).WithMany().HasForeignKey((IceReport x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((IceReport x) => x.Member).WithMany().HasForeignKey((IceReport x) => x.MemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
