using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserRefahBetaConf : IEntityTypeConfiguration<UserRefahBeta>
{
	public void Configure(EntityTypeBuilder<UserRefahBeta> builder)
	{
		builder.HasKey((UserRefahBeta p) => p.Id);
		builder.Property((UserRefahBeta x) => x.Message).HasMaxLength(250);
		builder.HasOne((UserRefahBeta x) => x.User).WithMany().HasForeignKey((UserRefahBeta x) => x.UserId)
			.OnDelete(DeleteBehavior.Restrict);
		builder.HasOne((UserRefahBeta x) => x.Customer).WithMany().HasForeignKey((UserRefahBeta x) => x.CustomerId)
			.OnDelete(DeleteBehavior.NoAction);
	}
}
