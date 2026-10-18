using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserLoanRequestAuthorizationItemAttachmentConf : IEntityTypeConfiguration<UserLoanRequestAuthorizationItemAttachment>
{
    public void Configure(EntityTypeBuilder<UserLoanRequestAuthorizationItemAttachment> builder)
    {
        builder.HasKey((UserLoanRequestAuthorizationItemAttachment p) => p.Id);
        builder.Property((UserLoanRequestAuthorizationItemAttachment x) => x.Attachment).HasMaxLength(100);
        builder.Property((UserLoanRequestAuthorizationItemAttachment x) => x.MemberDescription).HasMaxLength(1000);
        builder.HasOne((UserLoanRequestAuthorizationItemAttachment x) => x.UserLoanRequestAuthorizationItem).WithMany((UserLoanRequestAuthorizationItem x) => x.Attachments).HasForeignKey((UserLoanRequestAuthorizationItemAttachment x) => x.UserLoanRequestAuthorizationItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
