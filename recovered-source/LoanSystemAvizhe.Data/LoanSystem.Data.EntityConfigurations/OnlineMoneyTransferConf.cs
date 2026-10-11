using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class OnlineMoneyTransferConf : IEntityTypeConfiguration<OnlineMoneyTransfer>
{
	public void Configure(EntityTypeBuilder<OnlineMoneyTransfer> builder)
	{
		builder.Property((OnlineMoneyTransfer x) => x.From).HasMaxLength(50);
		builder.Property((OnlineMoneyTransfer x) => x.To).HasMaxLength(50);
		builder.Property((OnlineMoneyTransfer x) => x.OTP).HasMaxLength(10);
		builder.Property((OnlineMoneyTransfer x) => x.OurTraceNumber).HasMaxLength(50);
		builder.Property((OnlineMoneyTransfer x) => x.Cif).HasMaxLength(50);
		builder.HasOne((OnlineMoneyTransfer x) => x.Transaction).WithMany((Transaction x) => x.OnlineMoneyTransfers).HasForeignKey((OnlineMoneyTransfer e) => e.TransactionId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
		builder.HasOne((OnlineMoneyTransfer x) => x.Customer).WithMany().HasForeignKey((OnlineMoneyTransfer x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(required: false);
	}
}
