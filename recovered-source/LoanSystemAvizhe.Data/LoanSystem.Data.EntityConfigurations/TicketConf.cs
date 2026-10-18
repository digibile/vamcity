using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class TicketConf : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey((Ticket p) => p.Id);
        builder.Property((Ticket p) => p.Code).ValueGeneratedOnAdd().UseIdentityColumn(10000)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.Property((Ticket x) => x.Title).HasMaxLength(250);
        builder.HasOne((Ticket x) => x.UserLoan).WithMany().HasForeignKey((Ticket x) => x.UserLoanId)
            .IsRequired(required: false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((Ticket x) => x.UserLoanRequest).WithMany().HasForeignKey((Ticket x) => x.UserLoanRequestId)
            .IsRequired(required: false)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((Ticket x) => x.CreatedBy).WithMany().HasForeignKey((Ticket x) => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((Ticket x) => x.Customer).WithMany().HasForeignKey((Ticket x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
