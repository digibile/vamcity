using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class MemberAuthorizationItemAttachmentConf : IEntityTypeConfiguration<MemberAuthorizationItemAttachment>
{
	public void Configure(EntityTypeBuilder<MemberAuthorizationItemAttachment> builder)
	{
		builder.HasKey((MemberAuthorizationItemAttachment p) => p.Id);
		builder.Property((MemberAuthorizationItemAttachment x) => x.Attachment).HasMaxLength(100);
		builder.Property((MemberAuthorizationItemAttachment x) => x.MemberDescription).HasMaxLength(1000);
		builder.HasOne((MemberAuthorizationItemAttachment x) => x.MemberAuthorizationItem).WithMany((MemberAuthorizationItem x) => x.Attachments).HasForeignKey((MemberAuthorizationItemAttachment x) => x.MemberAuthorizationItemId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
