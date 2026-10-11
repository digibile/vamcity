using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AssuranceChequeItemConf : IEntityTypeConfiguration<AssuranceChequeItem>
{
	public void Configure(EntityTypeBuilder<AssuranceChequeItem> builder)
	{
		builder.HasKey((AssuranceChequeItem p) => p.Id);
		builder.Property((AssuranceChequeItem x) => x.ChequeNumber).HasMaxLength(20);
		builder.Property((AssuranceChequeItem x) => x.SayadiId).HasMaxLength(25);
		builder.Property((AssuranceChequeItem x) => x.FileName).HasMaxLength(50);
		builder.HasOne((AssuranceChequeItem x) => x.Assurance).WithMany((Assurance x) => x.AssuranceChequeItems).HasForeignKey((AssuranceChequeItem x) => x.AssuranceId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
