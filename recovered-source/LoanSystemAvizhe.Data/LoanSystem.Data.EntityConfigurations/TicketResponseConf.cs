using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class TicketResponseConf : IEntityTypeConfiguration<TicketResponse>
{
    public void Configure(EntityTypeBuilder<TicketResponse> builder)
    {
        builder.HasKey((TicketResponse p) => p.Id);
        builder.Property((TicketResponse x) => x.FileName).HasMaxLength(100);
        builder.HasOne((TicketResponse x) => x.CreatedBy).WithMany().HasForeignKey((TicketResponse x) => x.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne((TicketResponse x) => x.Ticket).WithMany((Ticket x) => x.TicketResponses).HasForeignKey((TicketResponse x) => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((TicketResponse x) => x.Customer).WithMany().HasForeignKey((TicketResponse x) => x.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
