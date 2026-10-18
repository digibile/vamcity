using LoanSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoanSystem.Data.EntityConfigurations;

internal class AppSettingsConf : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.HasKey((AppSettings p) => p.Id);
        builder.Property((AppSettings x) => x.ZarinpalMerchantCode).HasMaxLength(200);
        builder.Property((AppSettings x) => x.Shaba).HasMaxLength(50);
        builder.Property((AppSettings x) => x.BankInfo).HasMaxLength(1000);
        builder.Property((AppSettings x) => x.DashboardSettingsJson);
        builder.Property((AppSettings x) => x.ThemeSettingsJson).HasMaxLength(1000);
        builder.Property((AppSettings x) => x.IceConfigJson).HasMaxLength(1000);
        builder.Property((AppSettings x) => x.IPGParsianLoginAccount).HasMaxLength(200);
        builder.Property((AppSettings x) => x.Address).HasMaxLength(500);
        builder.Property((AppSettings x) => x.ShopLink).HasMaxLength(500);
        builder.Property((AppSettings x) => x.ShopTitle).HasMaxLength(250);
        builder.Property((AppSettings x) => x.InstagramLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.RubikaLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.TelegramLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.AparatLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.WhatsappLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.ApplicationLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.BusinessTitle).HasMaxLength(250);
        builder.Property((AppSettings x) => x.EnamadLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.EtehadieLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.SamandehiLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.AnjomanSenfiLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.KasboKarhayeInternetiTehranLink).HasMaxLength(250);
        builder.Property((AppSettings x) => x.ApiPassword).HasMaxLength(50);
        builder.Property((AppSettings x) => x.AgentOfficePhone).HasMaxLength(200);
        builder.Property((AppSettings x) => x.Phones).HasMaxLength(200);
        builder.Property((AppSettings x) => x.DepositChargeDueDateStatuses).HasMaxLength(1000);
        builder.Property((AppSettings x) => x.DepositChargeDueDateStatuses).HasMaxLength(1000);
        builder.HasOne((AppSettings x) => x.Customer).WithMany().HasForeignKey((AppSettings x) => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
