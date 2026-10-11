using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class ThirdPartySettingsConf : IEntityTypeConfiguration<ThirdPartySettings>
{
	public void Configure(EntityTypeBuilder<ThirdPartySettings> builder)
	{
		builder.HasKey((ThirdPartySettings p) => p.Id);
		builder.HasOne((ThirdPartySettings x) => x.Customer).WithMany().HasForeignKey((ThirdPartySettings x) => x.CustomerId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
