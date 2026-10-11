using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class MemberAuthorizationItemConf : IEntityTypeConfiguration<MemberAuthorizationItem>
{
	public void Configure(EntityTypeBuilder<MemberAuthorizationItem> builder)
	{
		builder.HasKey((MemberAuthorizationItem p) => p.Id);
		builder.Property((MemberAuthorizationItem x) => x.MemberDescription).HasMaxLength(10000);
		builder.Property((MemberAuthorizationItem x) => x.AdminDescription).HasMaxLength(1000);
		builder.Property((MemberAuthorizationItem x) => x.LastChangeBy).HasMaxLength(100);
		builder.HasOne((MemberAuthorizationItem x) => x.Member).WithMany().HasForeignKey((MemberAuthorizationItem x) => x.MemberId)
			.OnDelete(DeleteBehavior.NoAction);
		builder.HasOne((MemberAuthorizationItem x) => x.AuthorizationItem).WithMany().HasForeignKey((MemberAuthorizationItem x) => x.AuthorizationItemId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
