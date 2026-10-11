using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class UserConf : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.Property((User x) => x.Phone).HasMaxLength(20);
		builder.Property((User x) => x.BusinessName).HasMaxLength(50);
		builder.Property((User x) => x.PostalCode).HasMaxLength(20);
		builder.Property((User x) => x.Shenasname).HasMaxLength(20);
		builder.Property((User x) => x.City).HasMaxLength(50);
		builder.Property((User x) => x.UserCode).HasMaxLength(50);
		builder.HasOne((User x) => x.Reliability).WithMany().HasForeignKey((User x) => x.ReliabilityId)
			.OnDelete(DeleteBehavior.NoAction)
			.IsRequired(required: false);
		builder.HasOne((User x) => x.Agent).WithMany().HasForeignKey((User x) => x.AgentId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((User x) => x.CustomerMarketer).WithMany().HasForeignKey((User x) => x.CustomerMarketerId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((User x) => x.AgentMetaData).WithOne((AgentMetaData x) => x.User).HasForeignKey((AgentMetaData x) => x.UserId)
			.IsRequired(required: false)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
