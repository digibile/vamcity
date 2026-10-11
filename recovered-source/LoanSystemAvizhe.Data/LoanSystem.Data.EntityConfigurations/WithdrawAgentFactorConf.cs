using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class WithdrawAgentFactorConf : IEntityTypeConfiguration<WithdrawAgentFactor>
{
	public void Configure(EntityTypeBuilder<WithdrawAgentFactor> builder)
	{
		builder.HasKey((WithdrawAgentFactor p) => p.Id);
		builder.HasOne((WithdrawAgentFactor x) => x.Withdraw).WithMany().HasForeignKey((WithdrawAgentFactor x) => x.WithdrawId)
			.OnDelete(DeleteBehavior.NoAction);
		builder.HasOne((WithdrawAgentFactor x) => x.AgentFactor).WithMany((AgentFactor x) => x.WithdrawAgentFactors).HasForeignKey((WithdrawAgentFactor x) => x.AgentFactorId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
