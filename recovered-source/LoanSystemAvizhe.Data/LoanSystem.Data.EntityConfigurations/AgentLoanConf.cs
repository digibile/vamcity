using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AgentLoanConf : IEntityTypeConfiguration<AgentLoan>
{
    public void Configure(EntityTypeBuilder<AgentLoan> builder)
    {
        builder.HasOne((AgentLoan x) => x.Agent).WithMany((User x) => x.AgentLoans).HasForeignKey((AgentLoan x) => x.AgentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((AgentLoan x) => x.Loan).WithMany().HasForeignKey((AgentLoan x) => x.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne((AgentLoan x) => x.Contact).WithMany().HasForeignKey((AgentLoan x) => x.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
