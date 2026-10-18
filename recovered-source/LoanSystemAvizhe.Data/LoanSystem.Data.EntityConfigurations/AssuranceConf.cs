using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AssuranceConf : IEntityTypeConfiguration<Assurance>
{
    public void Configure(EntityTypeBuilder<Assurance> builder)
    {
        builder.HasKey((Assurance p) => p.Id);
        builder.Property((Assurance x) => x.Description).HasMaxLength(2000);
        builder.Property((Assurance x) => x.FileName).HasMaxLength(50);
        builder.Property((Assurance x) => x.ReturnFileType).HasMaxLength(20);
        builder.Property((Assurance x) => x.ReturnFileName).HasMaxLength(50);
        builder.Property((Assurance x) => x.FileType).HasMaxLength(20);
        builder.Property((Assurance x) => x.PersonFullName).HasMaxLength(50);
        builder.Property((Assurance x) => x.PersonNationalCode).HasMaxLength(20);
        builder.Property((Assurance x) => x.PersonalCode).HasMaxLength(50);
        builder.Property((Assurance x) => x.PersonMobile).HasMaxLength(50);
        builder.Property((Assurance x) => x.ReturnedToPerson).HasMaxLength(50);
        builder.Property((Assurance x) => x.GoldWeight).HasColumnType("decimal(18,4)");
        builder.HasOne((Assurance x) => x.UserLoan).WithMany((UserLoan x) => x.Assurances).HasForeignKey((Assurance x) => x.UserLoanId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne((Assurance x) => x.Organization).WithMany().HasForeignKey((Assurance x) => x.OrganizationId)
            .IsRequired(required: false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
