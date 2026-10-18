using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class IranValidityReportConf : IEntityTypeConfiguration<IranValidityReport>
{
    public void Configure(EntityTypeBuilder<IranValidityReport> builder)
    {
        builder.HasKey((IranValidityReport p) => p.Id);
        builder.Property((IranValidityReport x) => x.Result).HasMaxLength(20);
        builder.Property((IranValidityReport x) => x.Url).HasMaxLength(500);
        builder.Property((IranValidityReport x) => x.Status).HasMaxLength(100);
        builder.Property((IranValidityReport x) => x.AttachmentFileName).HasMaxLength(200);
        builder.Property((IranValidityReport x) => x.ReportPdfFileName).HasMaxLength(200);
        builder.Property((IranValidityReport x) => x.ReportID).HasMaxLength(200);
        builder.Property((IranValidityReport x) => x.IranValidityReportJson).HasMaxLength(1000);
        builder.Property((IranValidityReport x) => x.Messages).HasMaxLength(200);
        builder.HasOne((IranValidityReport x) => x.Customer).WithMany().HasForeignKey((IranValidityReport x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((IranValidityReport x) => x.Member).WithMany().HasForeignKey((IranValidityReport x) => x.MemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
