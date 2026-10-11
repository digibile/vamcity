using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class EmzameContractAuthorizationItemConf : IEntityTypeConfiguration<EmzameContractAuthorizationItem>
{
	public void Configure(EntityTypeBuilder<EmzameContractAuthorizationItem> builder)
	{
		builder.HasKey((EmzameContractAuthorizationItem p) => p.Id);
		builder.Property((EmzameContractAuthorizationItem p) => p.Tag).HasMaxLength(100);
		builder.HasOne((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItem).WithOne((UserLoanRequestAuthorizationItem x) => x.EmzameContractAuthorizationItem).HasForeignKey((EmzameContractAuthorizationItem x) => x.UserLoanRequestAuthorizationItemId)
			.OnDelete(DeleteBehavior.Cascade);
		builder.HasOne((EmzameContractAuthorizationItem x) => x.User).WithMany().HasForeignKey((EmzameContractAuthorizationItem x) => x.UserId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
