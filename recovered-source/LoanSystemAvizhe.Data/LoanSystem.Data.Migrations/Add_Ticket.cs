using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace LoanSystem.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20250216035645_Add_Ticket")]
public class Add_Ticket : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Tickets", (ColumnsBuilder table) =>
        {
            OperationBuilder<AddColumnOperation> id = table.Column<Guid>("uniqueidentifier");
            OperationBuilder<AddColumnOperation> code = table.Column<int>("int").Annotation("SqlServer:Identity", "10000, 1");
            OperationBuilder<AddColumnOperation> createdAt = table.Column<DateTime>("datetime2");
            int? maxLength = 250;
            return new
            {
                Id = id,
                Code = code,
                CreatedAt = createdAt,
                Title = table.Column<string>("nvarchar(250)", null, maxLength, rowVersion: false, null, nullable: true),
                Status = table.Column<int>("int"),
                Role = table.Column<int>("int"),
                ResponseCount = table.Column<int>("int"),
                AsMessage = table.Column<bool>("bit"),
                CreatedById = table.Column<long>("bigint"),
                TargetRole = table.Column<int>("int"),
                TargetUserId = table.Column<long>("bigint", null, null, rowVersion: false, null, nullable: true),
                UserLoanId = table.Column<Guid>("uniqueidentifier", null, null, rowVersion: false, null, nullable: true),
                UserLoanRequestId = table.Column<Guid>("uniqueidentifier", null, null, rowVersion: false, null, nullable: true),
                CustomerId = table.Column<Guid>("uniqueidentifier", null, null, rowVersion: false, null, nullable: true)
            };
        }, "dbo", table =>
        {
            table.PrimaryKey("PK_Tickets", x => x.Id);
            table.ForeignKey("FK_Tickets_AspNetUsers_CreatedById", x => x.CreatedById, "AspNetUsers", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Restrict);
            table.ForeignKey("FK_Tickets_AspNetUsers_TargetUserId", x => x.TargetUserId, "AspNetUsers", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Restrict);
            table.ForeignKey("FK_Tickets_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", "dbo");
            table.ForeignKey("FK_Tickets_UserLoanRequests_UserLoanRequestId", x => x.UserLoanRequestId, "UserLoanRequests", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Restrict);
            table.ForeignKey("FK_Tickets_UserLoans_UserLoanId", x => x.UserLoanId, "UserLoans", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Restrict);
        });
        migrationBuilder.CreateTable("TicketResponses", (ColumnsBuilder table) =>
        {
            OperationBuilder<AddColumnOperation> id = table.Column<Guid>("uniqueidentifier");
            OperationBuilder<AddColumnOperation> createdAt = table.Column<DateTime>("datetime2");
            OperationBuilder<AddColumnOperation> description = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            int? maxLength = 100;
            return new
            {
                Id = id,
                CreatedAt = createdAt,
                Description = description,
                FileName = table.Column<string>("nvarchar(100)", null, maxLength, rowVersion: false, null, nullable: true),
                FromAdmin = table.Column<bool>("bit"),
                IsSeen = table.Column<bool>("bit"),
                TicketId = table.Column<Guid>("uniqueidentifier"),
                Role = table.Column<int>("int"),
                CreatedById = table.Column<long>("bigint"),
                CustomerId = table.Column<Guid>("uniqueidentifier", null, null, rowVersion: false, null, nullable: true)
            };
        }, "dbo", table =>
        {
            table.PrimaryKey("PK_TicketResponses", x => x.Id);
            table.ForeignKey("FK_TicketResponses_AspNetUsers_CreatedById", x => x.CreatedById, "AspNetUsers", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Restrict);
            table.ForeignKey("FK_TicketResponses_Customers_CustomerId", x => x.CustomerId, "Customers", "Id", "dbo");
            table.ForeignKey("FK_TicketResponses_Tickets_TicketId", x => x.TicketId, "Tickets", "Id", "dbo", ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_TicketResponses_CreatedById", "TicketResponses", "CreatedById", "dbo");
        migrationBuilder.CreateIndex("IX_TicketResponses_CustomerId", "TicketResponses", "CustomerId", "dbo");
        migrationBuilder.CreateIndex("IX_TicketResponses_TicketId", "TicketResponses", "TicketId", "dbo");
        migrationBuilder.CreateIndex("IX_Tickets_CreatedById", "Tickets", "CreatedById", "dbo");
        migrationBuilder.CreateIndex("IX_Tickets_CustomerId", "Tickets", "CustomerId", "dbo");
        migrationBuilder.CreateIndex("IX_Tickets_TargetUserId", "Tickets", "TargetUserId", "dbo");
        migrationBuilder.CreateIndex("IX_Tickets_UserLoanId", "Tickets", "UserLoanId", "dbo");
        migrationBuilder.CreateIndex("IX_Tickets_UserLoanRequestId", "Tickets", "UserLoanRequestId", "dbo");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("TicketResponses", "dbo");
        migrationBuilder.DropTable("Tickets", "dbo");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo").UseIdentityColumns().HasAnnotation("Relational:MaxIdentifierLength", 128)
            .HasAnnotation("ProductVersion", "5.0.2");
        modelBuilder.Entity("LoanSystem.Data.Entities.AffiliateCommission", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<int>("Amount").HasColumnType("int");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<int>("PaidToUser").HasColumnType("int");
            b.Property<Guid>("PurchaseId").HasColumnType("uniqueidentifier");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.HasKey("Id");
            b.HasIndex("PurchaseId");
            b.HasIndex("UserId");
            b.ToTable("AffiliateCommissions");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentLoan", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<long>("AgentId").HasColumnType("bigint");
            b.Property<Guid?>("ContactId").HasColumnType("uniqueidentifier");
            b.Property<decimal>("ContactPercent").HasColumnType("decimal(18,2)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<byte>("DefaultCheckoutDays").HasColumnType("tinyint");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.Property<decimal>("Percent").HasColumnType("decimal(18,2)");
            b.Property<byte>("ProductCheckoutDays").HasColumnType("tinyint");
            b.Property<decimal>("ProductPercent").HasColumnType("decimal(18,2)");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("ContactId");
            b.HasIndex("CustomerId");
            b.HasIndex("LoanId");
            b.ToTable("AgentLoans");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentLoanAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("AuthorizationItemId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("AuthorizationItemId");
            b.HasIndex("CustomerId");
            b.HasIndex("LoanId");
            b.ToTable("AgentLoanAuthorizationItem");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentMetaData", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AgentCategoryActivities").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<int>("AgentType").HasColumnType("int");
            b.Property<string>("AsasnameFileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("CheckedBy").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("CompanyName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("EjarenameFileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("IsOnline").HasColumnType("bit");
            b.Property<bool>("IsPhysical").HasColumnType("bit");
            b.Property<string>("JavazKasbFileName").HasColumnType("nvarchar(max)");
            b.Property<string>("KartMelliFileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<DateTime>("LastAdminChangeDateTime").HasColumnType("datetime2");
            b.Property<string>("RejectReason").HasColumnType("nvarchar(max)");
            b.Property<string>("RoznameRasmiFileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("ShenaseMelli").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("ShomareSabt").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("SignaturesOwnerNames").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("SiteUrl").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<string>("ThumbnailFileName").HasColumnType("nvarchar(max)");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("UserId").IsUnique();
            b.ToTable("AgentMetaData");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AppSettings", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Address").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("AgentOfficePhone").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("AnjomanSenfiLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("AparatLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("ApiKey").HasColumnType("nvarchar(max)");
            b.Property<string>("ApiPassword").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<int>("AppendToWhichInstallment").HasColumnType("int");
            b.Property<string>("ApplicationLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("BankAccountsJson").HasColumnType("nvarchar(max)");
            b.Property<string>("BankGatewayRouterBaseUrl").HasColumnType("nvarchar(max)");
            b.Property<string>("BankInfo").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid?>("BeforeDueDateSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<int>("BeforeDueDaysCount").HasColumnType("int");
            b.Property<string>("BusinessTitle").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("DashboardSettingsJson").HasColumnType("nvarchar(max)");
            b.Property<int>("DefaultSmsPanel").HasColumnType("int");
            b.Property<string>("DepositChargeDueDateStatuses").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid?>("DueDateRememberSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<TimeSpan?>("DueDatedRememberSmsTime").HasColumnType("time");
            b.Property<string>("Email").HasColumnType("nvarchar(max)");
            b.Property<string>("EnamadLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("EtehadieLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("FinnoTechConfigJson").HasColumnType("nvarchar(max)");
            b.Property<string>("IPGParsianLoginAccount").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("IceConfigJson").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("InstagramLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("IranValidityConfigJson").HasColumnType("nvarchar(max)");
            b.Property<bool>("IsDueDateSmsActive").HasColumnType("bit");
            b.Property<bool>("IsOnlinePaymentActive").HasColumnType("bit");
            b.Property<bool>("IsRememberSmsActive").HasColumnType("bit");
            b.Property<string>("KasboKarhayeInternetiTehranLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("KaveNegarConfigJson").HasColumnType("nvarchar(max)");
            b.Property<Guid?>("LoginSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<string>("MelliPayamakConfigJson").HasColumnType("nvarchar(max)");
            b.Property<string>("NextPayApiKey").HasColumnType("nvarchar(max)");
            b.Property<int>("OnlinePaymentGateway").HasColumnType("int");
            b.Property<Guid?>("OverDueDateSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<bool>("OverDueDayRemeberOnce").HasColumnType("bit");
            b.Property<int>("OverDueDaysCount").HasColumnType("int");
            b.Property<string>("Phones").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<Guid?>("RegisterOtpSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("RegisterSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<byte>("RoundCountNumber").HasColumnType("tinyint");
            b.Property<string>("RubikaLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("SadadMerchantId").HasColumnType("nvarchar(max)");
            b.Property<string>("SadadMerchantKey").HasColumnType("nvarchar(max)");
            b.Property<string>("SadadTerminalId").HasColumnType("nvarchar(max)");
            b.Property<string>("SamandehiLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("SepTerminalId").HasColumnType("nvarchar(max)");
            b.Property<string>("Shaba").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("ShopLink").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("ShopTitle").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<int>("SmsCredit").HasColumnType("int");
            b.Property<string>("SponsoreDueDateStatuses").HasColumnType("nvarchar(max)");
            b.Property<string>("TelegramLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<Guid?>("ThanksPaymentSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<string>("ThemeSettingsJson").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid?>("WalletChargeSmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<string>("WhatsappLink").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("ZarinpalMerchantCode").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.HasKey("Id");
            b.HasIndex("BeforeDueDateSmsPatternId");
            b.HasIndex("CustomerId");
            b.HasIndex("DueDateRememberSmsPatternId");
            b.HasIndex("LoginSmsPatternId");
            b.HasIndex("OverDueDateSmsPatternId");
            b.HasIndex("RegisterOtpSmsPatternId");
            b.HasIndex("RegisterSmsPatternId");
            b.HasIndex("ThanksPaymentSmsPatternId");
            b.HasIndex("WalletChargeSmsPatternId");
            b.ToTable("AppSettings");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AppSms", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<int>("AppSmsType").HasColumnType("int");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<int>("DeliverCheckRetryCount").HasColumnType("int");
            b.Property<Guid>("EntityId").HasColumnType("uniqueidentifier");
            b.Property<string>("EntityType").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<decimal>("Price").HasColumnType("decimal(18,2)");
            b.Property<string>("SMSKey").HasColumnType("nvarchar(max)");
            b.Property<int>("SmsPanel").HasColumnType("int");
            b.Property<string>("SmsPatternTitle").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<string>("Tag").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Text").HasMaxLength(1500).HasColumnType("nvarchar(1500)");
            b.Property<string>("To").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("AppSms");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Assurance", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            b.Property<string>("FileName").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<long>("FileSize").HasColumnType("bigint");
            b.Property<string>("FileType").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserLoanId");
            b.ToTable("Assurance");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<int>("SystemType").HasColumnType("int");
            b.Property<string>("Title").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("UseIfAlreadyExist").HasColumnType("bit");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("AuthorizationItem");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Consultant", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("CallResult").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("Description").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<bool>("IsSeen").HasColumnType("bit");
            b.Property<string>("Name").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Phone").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.HasKey("Id");
            b.ToTable("Consultants");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Contact", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Address").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("BankInfo").HasColumnType("nvarchar(max)");
            b.Property<string>("BusinessName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("FullName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Phone").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("Contacts");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Customer", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AffCode").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<string>("Email").HasColumnType("nvarchar(max)");
            b.Property<DateTime>("ExpireDate").HasColumnType("datetime2");
            b.Property<bool>("FirstLoginDone").HasColumnType("bit");
            b.Property<string>("InitPassword").HasColumnType("nvarchar(max)");
            b.Property<string>("Mobile").HasColumnType("nvarchar(max)");
            b.Property<string>("Name").HasColumnType("nvarchar(max)");
            b.Property<string>("ParkDomain").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("Permanent").HasColumnType("bit");
            b.Property<bool>("Purchased").HasColumnType("bit");
            b.Property<DateTime>("RegDate").HasColumnType("datetime2");
            b.HasKey("Id");
            b.ToTable("Customers");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.CustomerMarketerCommission", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<long>("MarketerId").HasColumnType("bigint");
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("MarketerId");
            b.HasIndex("UserLoanId");
            b.ToTable("CustomerMarketerCommissions");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.CustomerPurchasePlan", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("EndDate").HasColumnType("datetime2");
            b.Property<int>("MaxLoanCount").HasColumnType("int");
            b.Property<Guid?>("PurchaseId").HasColumnType("uniqueidentifier");
            b.Property<int>("RemainLoanCount").HasColumnType("int");
            b.Property<DateTime>("StartDate").HasColumnType("datetime2");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("PurchaseId");
            b.ToTable("CustomerPurchasePlan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Expense", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<string>("Comment").HasColumnType("nvarchar(max)");
            b.Property<DateTime>("CostDate").HasColumnType("datetime2");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
            b.Property<string>("Title").HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("Expenses");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.IceReport", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AttachmentFileName").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<string>("Errors").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("IceReportJson").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<long>("MemberId").HasColumnType("bigint");
            b.Property<decimal>("Price").HasColumnType("decimal(18,2)");
            b.Property<string>("ReportID").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("ReportPdfFileName").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("Result").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<string>("Status").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<string>("Url").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("MemberId");
            b.ToTable("IceReport");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.IranValidityReport", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AttachmentFileName").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<int>("GradeNumber").HasColumnType("int");
            b.Property<string>("IranValidityReportJson").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<long>("MemberId").HasColumnType("bigint");
            b.Property<string>("Messages").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<decimal>("Price").HasColumnType("decimal(18,2)");
            b.Property<string>("ReportID").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("ReportPdfFileName").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("Result").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<int>("Status").HasMaxLength(100).HasColumnType("int");
            b.Property<string>("Url").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("MemberId");
            b.ToTable("IranValidityReport");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AllowedInstallments").HasColumnType("nvarchar(max)");
            b.Property<int>("Code").HasColumnType("int");
            b.Property<string>("CostsJson").HasColumnType("nvarchar(max)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<int>("DefaultSponsoreCheckoutDays").HasColumnType("int");
            b.Property<double>("DelayPercent").HasColumnType("float");
            b.Property<bool>("Disable").HasColumnType("bit");
            b.Property<string>("DisableText").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<double>("EarlyPercent").HasColumnType("float");
            b.Property<string>("FileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("HasSponsore").HasColumnType("bit");
            b.Property<bool>("IncludePlan").HasColumnType("bit");
            b.Property<bool>("IsSmsActive").HasColumnType("bit");
            b.Property<bool>("IsSponsored").HasColumnType("bit");
            b.Property<int>("LoanProfitType").HasColumnType("int");
            b.Property<decimal>("MaxAmount").HasColumnType("decimal(18,2)");
            b.Property<int>("MaxMonthGap").HasColumnType("int");
            b.Property<string>("PlanHtml").HasMaxLength(5000).HasColumnType("nvarchar(max)");
            b.Property<string>("PlanShortDescription").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("PlanTitle").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<double>("ProfitPercent").HasColumnType("float");
            b.Property<string>("Title").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("Loans");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("AuthorizationItemId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<bool>("IsPrimary").HasColumnType("bit");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("AuthorizationItemId");
            b.HasIndex("CustomerId");
            b.HasIndex("LoanId");
            b.ToTable("LoanAuthorizationItems");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanContact", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("ContactId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.Property<decimal>("Share").HasColumnType("decimal(18,2)");
            b.HasKey("Id");
            b.HasIndex("ContactId");
            b.HasIndex("CustomerId");
            b.HasIndex("LoanId");
            b.ToTable("LoanContacts");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanRequestStatus", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<bool>("AllowNewRequest").HasColumnType("bit");
            b.Property<bool>("Changable").HasColumnType("bit");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<int>("Order").HasColumnType("int");
            b.Property<Guid?>("SmsPatternId").HasColumnType("uniqueidentifier");
            b.Property<int>("StatusGroup").HasColumnType("int");
            b.Property<int>("StepCategory").HasColumnType("int");
            b.Property<int?>("SystemAgentRequestStatus").HasColumnType("int");
            b.Property<int?>("SystemRequestStatus").HasColumnType("int");
            b.Property<string>("Title").IsRequired().HasMaxLength(50)
                .HasColumnType("nvarchar(50)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("SmsPatternId");
            b.ToTable("LoanRequestStatuses");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.MemberPurchase", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<long?>("CreatorUserId").HasColumnType("bigint");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<string>("IPGParsianJson").HasColumnType("nvarchar(max)");
            b.Property<bool>("IsPaymentSuccess").HasColumnType("bit");
            b.Property<int>("MemberPurchaseType").HasColumnType("int");
            b.Property<string>("NextPayJson").HasColumnType("nvarchar(max)");
            b.Property<int>("OnlinePaymentGateway").HasColumnType("int");
            b.Property<int>("PaymentStep").HasColumnType("int");
            b.Property<string>("SadadJson").HasColumnType("nvarchar(max)");
            b.Property<string>("SepJson").HasMaxLength(5000).HasColumnType("nvarchar(max)");
            b.Property<DateTime>("StartDate").HasColumnType("datetime2");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<Guid?>("UserLoanPaymentId").HasColumnType("uniqueidentifier");
            b.Property<string>("ZarinPalJson").HasMaxLength(5000).HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("CreatorUserId");
            b.HasIndex("UserId");
            b.HasIndex("UserLoanPaymentId");
            b.ToTable("MemberPurchases");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.OffDate", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<string>("Title").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("OffDate");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.OnlineMoneyTransfer", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<string>("Cif").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("EFardaJson").HasColumnType("nvarchar(max)");
            b.Property<string>("From").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<bool>("IsSuccess").HasColumnType("bit");
            b.Property<string>("OTP").HasMaxLength(10).HasColumnType("nvarchar(10)");
            b.Property<DateTime>("OTPGenerateDateTime").HasColumnType("datetime2");
            b.Property<Guid?>("OnlineMoneyTransferPanelId").HasColumnType("uniqueidentifier");
            b.Property<int>("OnlineMoneyTransferStep").HasColumnType("int");
            b.Property<string>("OurTraceNumber").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("To").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("ToName").HasColumnType("nvarchar(max)");
            b.Property<Guid?>("TransactionId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("TransactionId");
            b.ToTable("OnlineMoneyTransfer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Purchase", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AffCode").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.Property<bool>("IsPaymentSuccess").HasColumnType("bit");
            b.Property<int>("LoanCount").HasColumnType("int");
            b.Property<int>("PaymentMethod").HasColumnType("int");
            b.Property<int>("PaymentStep").HasColumnType("int");
            b.Property<int>("ProductType").HasColumnType("int");
            b.Property<DateTime>("StartDate").HasColumnType("datetime2");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<string>("ZarinPalAuthority").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("ZarinPalRefId").HasColumnType("nvarchar(max)");
            b.Property<int>("ZarinPalStatus").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("UserId");
            b.ToTable("Purchases");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Role", (EntityTypeBuilder b) =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint")
                .UseIdentityColumn();
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
            b.Property<string>("Name").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<string>("NormalizedName").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.HasKey("Id");
            b.HasIndex("NormalizedName").IsUnique().HasDatabaseName("RoleNameIndex")
                .HasFilter("[NormalizedName] IS NOT NULL");
            b.ToTable("AspNetRoles");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.SmsPattern", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<int>("Category").HasColumnType("int");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<bool>("IsPatternBase").HasColumnType("bit");
            b.Property<bool>("IsSystem").HasColumnType("bit");
            b.Property<string>("Pattern").HasColumnType("nvarchar(max)");
            b.Property<string>("PatternBaseCode").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<string>("RejectReason").HasColumnType("nvarchar(max)");
            b.Property<int>("SmsPatternPanel").HasColumnType("int");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<string>("Title").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("SmsPatterns");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.SubAgentTransaction", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<string>("FileName").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<bool>("IsWithdraw").HasColumnType("bit");
            b.Property<int>("PaymentMethod").HasColumnType("int");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("SavedBy").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<Guid?>("UserLoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("UserLoanId");
            b.ToTable("SubAgentTransactions");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.ThirdPartySettings", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<int>("DefaultThirdParty").HasColumnType("int");
            b.Property<string>("DigibileSettingsJson").HasColumnType("nvarchar(max)");
            b.Property<string>("LandaSettingsJson").HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("ThirdPartySettings");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.ThirdPartyTransfer", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("DigibileJson").HasColumnType("nvarchar(max)");
            b.Property<string>("EntityId").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("EntityTag").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<string>("EntityType").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("LandaJson").HasColumnType("nvarchar(max)");
            b.Property<string>("NikanJson").HasColumnType("nvarchar(max)");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.ToTable("ThirdPartyTransfer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Ticket", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<bool>("AsMessage").HasColumnType("bit");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn(10000);
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<long>("CreatedById").HasColumnType("bigint");
            b.Property<Guid?>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<int>("ResponseCount").HasColumnType("int");
            b.Property<int>("Role").HasColumnType("int");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<int>("TargetRole").HasColumnType("int");
            b.Property<long?>("TargetUserId").HasColumnType("bigint");
            b.Property<string>("Title").HasMaxLength(250).HasColumnType("nvarchar(250)");
            b.Property<Guid?>("UserLoanId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("UserLoanRequestId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("CreatedById");
            b.HasIndex("CustomerId");
            b.HasIndex("TargetUserId");
            b.HasIndex("UserLoanId");
            b.HasIndex("UserLoanRequestId");
            b.ToTable("Tickets");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.TicketResponse", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<long>("CreatedById").HasColumnType("bigint");
            b.Property<Guid?>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasColumnType("nvarchar(max)");
            b.Property<string>("FileName").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("FromAdmin").HasColumnType("bit");
            b.Property<bool>("IsSeen").HasColumnType("bit");
            b.Property<int>("Role").HasColumnType("int");
            b.Property<Guid>("TicketId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("CreatedById");
            b.HasIndex("CustomerId");
            b.HasIndex("TicketId");
            b.ToTable("TicketResponses");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Transaction", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<long?>("AgentId").HasColumnType("bigint");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<Guid?>("ContactId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("CustomerMarketerCommissionId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasColumnType("nvarchar(max)");
            b.Property<string>("FileName").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("FileName2").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("FileName3").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<double>("GoldRate").HasColumnType("float");
            b.Property<double>("GoldWeight").HasColumnType("float");
            b.Property<bool>("IsUnknown").HasColumnType("bit");
            b.Property<bool>("IsWithdraw").HasColumnType("bit");
            b.Property<int>("PaymentMethod").HasColumnType("int");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
            b.Property<Guid?>("SponsoredLoanId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("SuccessfullOnlineMoneyTransferId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("UserLoanContactId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("UserLoanId").HasColumnType("uniqueidentifier");
            b.Property<double>("WeightBalance").HasColumnType("float");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("ContactId");
            b.HasIndex("CustomerId");
            b.HasIndex("CustomerMarketerCommissionId");
            b.HasIndex("SponsoredLoanId");
            b.HasIndex("UserLoanContactId");
            b.HasIndex("UserLoanId");
            b.ToTable("Withdraws");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
        {
            b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint")
                .UseIdentityColumn();
            b.Property<int>("AccessFailedCount").HasColumnType("int");
            b.Property<string>("Address").HasColumnType("nvarchar(max)");
            b.Property<string>("AffiliateCode").HasColumnType("nvarchar(max)");
            b.Property<long?>("AgentId").HasColumnType("bigint");
            b.Property<Guid?>("AgentMetaDataId").HasColumnType("uniqueidentifier");
            b.Property<string>("BankAccountsJson").HasColumnType("nvarchar(max)");
            b.Property<string>("BusinessName").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("City").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<int>("CommissionPercent").HasColumnType("int");
            b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
            b.Property<decimal>("Credit").HasColumnType("decimal(18,2)");
            b.Property<Guid?>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<long?>("CustomerMarketerId").HasColumnType("bigint");
            b.Property<string>("Email").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<bool>("EmailConfirmed").HasColumnType("bit");
            b.Property<string>("Familiar1FirstName").HasColumnType("nvarchar(max)");
            b.Property<string>("Familiar1LastName").HasColumnType("nvarchar(max)");
            b.Property<string>("Familiar1Mobile").HasColumnType("nvarchar(max)");
            b.Property<string>("Familiar2FirstName").HasColumnType("nvarchar(max)");
            b.Property<string>("Familiar2LastName").HasColumnType("nvarchar(max)");
            b.Property<string>("Familiar2Mobile").HasColumnType("nvarchar(max)");
            b.Property<string>("FatherName").HasColumnType("nvarchar(max)");
            b.Property<string>("FirstName").HasColumnType("nvarchar(max)");
            b.Property<bool>("Gender").HasColumnType("bit");
            b.Property<bool>("IsActive").HasColumnType("bit");
            b.Property<DateTime>("LastLoginDate").HasColumnType("datetime2");
            b.Property<DateTime>("LastLoginDate2").HasColumnType("datetime2");
            b.Property<string>("LastName").HasColumnType("nvarchar(max)");
            b.Property<bool>("LockoutEnabled").HasColumnType("bit");
            b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("datetimeoffset");
            b.Property<string>("Mobile").HasColumnType("nvarchar(max)");
            b.Property<bool>("MobileVerified").HasColumnType("bit");
            b.Property<string>("NationalCode").HasColumnType("nvarchar(max)");
            b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<int>("Otp").HasColumnType("int");
            b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
            b.Property<string>("Phone").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
            b.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");
            b.Property<string>("PostalCode").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<DateTime>("RegDate").HasColumnType("datetime2");
            b.Property<DateTime>("RegDate2").HasColumnType("datetime2");
            b.Property<bool>("RegistrationCompleted").HasColumnType("bit");
            b.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
            b.Property<string>("Shenasname").HasMaxLength(20).HasColumnType("nvarchar(20)");
            b.Property<bool>("SmsActiveForMembers").HasColumnType("bit");
            b.Property<decimal>("TotalSavedMoney").HasColumnType("decimal(18,2)");
            b.Property<bool>("TwoFactorEnabled").HasColumnType("bit");
            b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<int>("ViewCount").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("CustomerId");
            b.HasIndex("CustomerMarketerId");
            b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
            b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex")
                .HasFilter("[NormalizedUserName] IS NOT NULL");
            b.ToTable("AspNetUsers");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime?>("AgentDueDate").HasColumnType("datetime2");
            b.Property<long?>("AgentId").HasColumnType("bigint");
            b.Property<decimal>("AgentPrice").HasColumnType("decimal(18,2)");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<DateTime?>("CheckoutDate").HasColumnType("datetime2");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<Guid?>("CustomerPurchasePlanId").HasColumnType("uniqueidentifier");
            b.Property<DateTime?>("DepositChargeDueDate").HasColumnType("datetime2");
            b.Property<string>("Description").HasColumnType("nvarchar(max)");
            b.Property<decimal>("Installment").HasColumnType("decimal(18,2)");
            b.Property<bool?>("IsCheckouted").HasColumnType("bit");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.Property<int>("Month").HasColumnType("int");
            b.Property<int>("MonthGap").ValueGeneratedOnAdd().HasColumnType("int")
                .HasDefaultValue(1);
            b.Property<decimal>("Paid").HasColumnType("decimal(18,2)");
            b.Property<decimal>("PayableAmount").HasColumnType("decimal(18,2)");
            b.Property<decimal>("PreDepositAmount").HasColumnType("decimal(18,2)");
            b.Property<decimal>("Remain").HasColumnType("decimal(18,2)");
            b.Property<DateTime>("StartDate").HasColumnType("datetime2");
            b.Property<string>("Status").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<DateTime?>("SubAgentDueDate").HasColumnType("datetime2");
            b.Property<long?>("SubAgentId").HasColumnType("bigint");
            b.Property<decimal>("SubAgentPrice").HasColumnType("decimal(18,2)");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<Guid?>("UserLoanRequestId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("CustomerPurchasePlanId");
            b.HasIndex("LoanId");
            b.HasIndex("SubAgentId");
            b.HasIndex("UserId");
            b.HasIndex("UserLoanRequestId");
            b.ToTable("UserLoans");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanContact", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("ContactId").HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("DueDate").HasColumnType("datetime2");
            b.Property<decimal>("Share").HasColumnType("decimal(18,2)");
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("UserLoanId1").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("ContactId");
            b.HasIndex("CustomerId");
            b.HasIndex("UserLoanId");
            b.HasIndex("UserLoanId1");
            b.ToTable("UserLoanContact");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPayment", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<int>("DelayDay").HasColumnType("int");
            b.Property<decimal>("DelayPrice").HasColumnType("decimal(18,2)");
            b.Property<string>("Description").HasColumnType("nvarchar(max)");
            b.Property<string>("FileMonth").HasColumnType("nvarchar(max)");
            b.Property<double>("GoldRate").HasColumnType("float");
            b.Property<double>("GoldWeight").HasColumnType("float");
            b.Property<decimal>("InstallmentAmount").HasColumnType("decimal(18,2)");
            b.Property<int>("InstallmentNumber").HasColumnType("int");
            b.Property<bool>("IsGoldCheckouted").HasColumnType("bit");
            b.Property<int>("PaymentMethod").HasColumnType("int");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
            b.Property<bool>("SentDueDateSms").HasColumnType("bit");
            b.Property<bool>("SentOverDueDateSms").HasColumnType("bit");
            b.Property<DateTime>("TargetMonth").HasColumnType("datetime2");
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserLoanId");
            b.ToTable("UserLoanPayments");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPaymentRequest", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("AdminAcceptDate").HasColumnType("datetime2");
            b.Property<long?>("AdminAcceptorId").HasColumnType("bigint");
            b.Property<string>("AdminDescription").HasColumnType("nvarchar(max)");
            b.Property<bool>("AffectOnMainPayment").HasColumnType("bit");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<string>("FileName").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("MemberDescription").HasColumnType("nvarchar(max)");
            b.Property<DateTime>("PaymentDate").HasColumnType("datetime2");
            b.Property<int>("PaymentMethod").HasColumnType("int");
            b.Property<DateTime>("RequestDate").HasColumnType("datetime2");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<string>("TrackNumber").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<Guid>("UserLoanPaymentId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("AdminAcceptorId");
            b.HasIndex("UserLoanPaymentId");
            b.ToTable("UserLoanPaymentRequests");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequest", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AdminDescription").HasColumnType("nvarchar(max)");
            b.Property<string>("AdminDescriptionPrivate").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<long?>("AgentId").HasColumnType("bigint");
            b.Property<Guid?>("AgentStatusId").HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<int>("ChargeType").HasColumnType("int");
            b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<string>("Comment").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<decimal>("Installment").HasColumnType("decimal(18,2)");
            b.Property<int>("InstallmentCount").HasColumnType("int");
            b.Property<bool>("IsPhysicalDocsDelivered").HasColumnType("bit");
            b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
            b.Property<string>("LockedBy").HasColumnType("nvarchar(max)");
            b.Property<int>("Month").HasColumnType("int");
            b.Property<int>("MonthGap").HasColumnType("int");
            b.Property<DateTime>("RequestDate").HasColumnType("datetime2");
            b.Property<decimal>("SponsoreAmount").HasColumnType("decimal(18,2)");
            b.Property<DateTime>("SponsoreDueDate").HasColumnType("datetime2");
            b.Property<Guid?>("StatusId").HasColumnType("uniqueidentifier");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<int>("Year").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("AgentId");
            b.HasIndex("AgentStatusId");
            b.HasIndex("LoanId");
            b.HasIndex("StatusId");
            b.HasIndex("UserId");
            b.ToTable("UserLoanRequests");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("AdminDescription").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<string>("Attachment").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("AttachmentByAdmin").HasColumnType("bit");
            b.Property<Guid>("AuthorizationItemId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("CreateDate").HasColumnType("datetime2");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("LastChangeDate").HasColumnType("datetime2");
            b.Property<string>("MemberDescription").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<Guid>("UserLoanRequestId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("AuthorizationItemId");
            b.HasIndex("UserLoanRequestId");
            b.ToTable("UserLoanRequestAuthorizationItems");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItemAttachment", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Attachment").HasMaxLength(100).HasColumnType("nvarchar(100)");
            b.Property<bool>("AttachmentByAdmin").HasColumnType("bit");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("MemberDescription").HasMaxLength(1000).HasColumnType("nvarchar(1000)");
            b.Property<Guid>("UserLoanRequestAuthorizationItemId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserLoanRequestAuthorizationItemId");
            b.ToTable("UserLoanRequestAuthorizationItemAttachments");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestStatusHistory", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<DateTime>("DateTime").HasColumnType("datetime2");
            b.Property<Guid?>("FromAgentStatusId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("FromStatusId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("ToAgentStatusId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("ToStatusId").HasColumnType("uniqueidentifier");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<Guid>("UserLoanRequestId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("FromAgentStatusId");
            b.HasIndex("FromStatusId");
            b.HasIndex("ToAgentStatusId");
            b.HasIndex("ToStatusId");
            b.HasIndex("UserId");
            b.HasIndex("UserLoanRequestId");
            b.ToTable("UserLoanRequestStatusHistories");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanSponsoreDueDate", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
            b.Property<string>("Description").HasMaxLength(500).HasColumnType("nvarchar(500)");
            b.Property<DateTime>("DueDate").HasColumnType("datetime2");
            b.Property<bool>("IsCheckouted").HasColumnType("bit");
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.Property<Guid?>("UserLoanId1").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("CustomerId");
            b.HasIndex("UserLoanId");
            b.HasIndex("UserLoanId1");
            b.ToTable("UserLoanSponsoreDueDate");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserSavedMoney", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
            b.Property<string>("Description").HasColumnType("nvarchar(max)");
            b.Property<string>("FileMonth").HasColumnType("nvarchar(max)");
            b.Property<long>("MemberId").HasColumnType("bigint");
            b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
            b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
            b.Property<DateTime>("TargetMonth").HasColumnType("datetime2");
            b.HasKey("Id");
            b.HasIndex("MemberId");
            b.ToTable("UserSavedMonies");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.VisitorLog", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<DateTime>("Date").HasColumnType("datetime2");
            b.Property<string>("IpAddress").HasMaxLength(50).HasColumnType("nvarchar(50)");
            b.Property<string>("Mrk").HasColumnType("nvarchar(max)");
            b.Property<string>("UserAgent").HasMaxLength(200).HasColumnType("nvarchar(200)");
            b.HasKey("Id");
            b.ToTable("VisitorLogs");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<long>", (EntityTypeBuilder b) =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
            b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
            b.Property<long>("RoleId").HasColumnType("bigint");
            b.HasKey("Id");
            b.HasIndex("RoleId");
            b.ToTable("AspNetRoleClaims");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<long>", (EntityTypeBuilder b) =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int")
                .UseIdentityColumn();
            b.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
            b.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.HasKey("Id");
            b.HasIndex("UserId");
            b.ToTable("AspNetUserClaims");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<long>", (EntityTypeBuilder b) =>
        {
            b.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
            b.Property<string>("ProviderKey").HasColumnType("nvarchar(450)");
            b.Property<string>("ProviderDisplayName").HasColumnType("nvarchar(max)");
            b.Property<long>("UserId").HasColumnType("bigint");
            b.HasKey("LoginProvider", "ProviderKey");
            b.HasIndex("UserId");
            b.ToTable("AspNetUserLogins");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<long>", (EntityTypeBuilder b) =>
        {
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<long>("RoleId").HasColumnType("bigint");
            b.HasKey("UserId", "RoleId");
            b.HasIndex("RoleId");
            b.ToTable("AspNetUserRoles");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<long>", (EntityTypeBuilder b) =>
        {
            b.Property<long>("UserId").HasColumnType("bigint");
            b.Property<string>("LoginProvider").HasColumnType("nvarchar(450)");
            b.Property<string>("Name").HasColumnType("nvarchar(450)");
            b.Property<string>("Value").HasColumnType("nvarchar(max)");
            b.HasKey("UserId", "LoginProvider", "Name");
            b.ToTable("AspNetUserTokens");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AffiliateCommission", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Purchase", "Purchase").WithMany("AffiliateCommissions").HasForeignKey("PurchaseId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Purchase");
            b.Navigation("User");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentLoan", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Agent").WithMany("AgentLoans").HasForeignKey("AgentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Contact", "Contact").WithMany().HasForeignKey("ContactId")
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany().HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Agent");
            b.Navigation("Contact");
            b.Navigation("Customer");
            b.Navigation("Loan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentLoanAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.AuthorizationItem", "AuthorizationItem").WithMany().HasForeignKey("AuthorizationItemId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany("AgentLoanAuthorizationItems").HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("AuthorizationItem");
            b.Navigation("Customer");
            b.Navigation("Loan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AgentMetaData", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithOne("AgentMetaData").HasForeignKey("LoanSystem.Data.Entities.AgentMetaData", "UserId")
                .OnDelete(DeleteBehavior.Cascade);
            b.Navigation("Customer");
            b.Navigation("User");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AppSettings", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "BeforeDueDateSmsPattern").WithMany().HasForeignKey("BeforeDueDateSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "DueDateRememberSmsPattern").WithMany().HasForeignKey("DueDateRememberSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "LoginSmsPattern").WithMany().HasForeignKey("LoginSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "OverDueDateSmsPattern").WithMany().HasForeignKey("OverDueDateSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "RegisterOtpSmsPattern").WithMany().HasForeignKey("RegisterOtpSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "RegisterSmsPattern").WithMany().HasForeignKey("RegisterSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "ThanksPaymentSmsPattern").WithMany().HasForeignKey("ThanksPaymentSmsPatternId");
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "WalletChargeSmsPattern").WithMany().HasForeignKey("WalletChargeSmsPatternId");
            b.Navigation("BeforeDueDateSmsPattern");
            b.Navigation("Customer");
            b.Navigation("DueDateRememberSmsPattern");
            b.Navigation("LoginSmsPattern");
            b.Navigation("OverDueDateSmsPattern");
            b.Navigation("RegisterOtpSmsPattern");
            b.Navigation("RegisterSmsPattern");
            b.Navigation("ThanksPaymentSmsPattern");
            b.Navigation("WalletChargeSmsPattern");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AppSms", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Assurance", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany("Assurances").HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.AuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Contact", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.CustomerMarketerCommission", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Marketer").WithMany().HasForeignKey("MarketerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Marketer");
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.CustomerPurchasePlan", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Purchase", "Purchase").WithMany().HasForeignKey("PurchaseId");
            b.Navigation("Customer");
            b.Navigation("Purchase");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Expense", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.IceReport", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "Member").WithMany().HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
            b.Navigation("Member");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.IranValidityReport", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "Member").WithMany().HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
            b.Navigation("Member");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.AuthorizationItem", "AuthorizationItem").WithMany().HasForeignKey("AuthorizationItemId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany("LoanAuthorizationItems").HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("AuthorizationItem");
            b.Navigation("Customer");
            b.Navigation("Loan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanContact", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Contact", "Contact").WithMany().HasForeignKey("ContactId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany("LoanContacts").HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Contact");
            b.Navigation("Customer");
            b.Navigation("Loan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.LoanRequestStatus", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.SmsPattern", "SmsPattern").WithMany().HasForeignKey("SmsPatternId")
                .OnDelete(DeleteBehavior.NoAction);
            b.Navigation("Customer");
            b.Navigation("SmsPattern");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.MemberPurchase", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "CreatorUser").WithMany().HasForeignKey("CreatorUserId");
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoanPayment", "UserLoanPayment").WithMany("MemberPurchases").HasForeignKey("UserLoanPaymentId")
                .OnDelete(DeleteBehavior.NoAction);
            b.Navigation("CreatorUser");
            b.Navigation("User");
            b.Navigation("UserLoanPayment");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.OffDate", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.OnlineMoneyTransfer", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Transaction", "Transaction").WithMany("OnlineMoneyTransfers").HasForeignKey("TransactionId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Customer");
            b.Navigation("Transaction");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Purchase", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany("Purchases").HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Customer");
            b.Navigation("User");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.SmsPattern", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.SubAgentTransaction", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Customer");
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.ThirdPartySettings", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.ThirdPartyTransfer", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Customer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Ticket", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "CreatedBy").WithMany().HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne("LoanSystem.Data.Entities.User", "TargetUser").WithMany().HasForeignKey("TargetUserId");
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.UserLoanRequest", "UserLoanRequest").WithMany().HasForeignKey("UserLoanRequestId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("CreatedBy");
            b.Navigation("Customer");
            b.Navigation("TargetUser");
            b.Navigation("UserLoan");
            b.Navigation("UserLoanRequest");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.TicketResponse", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "CreatedBy").WithMany().HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne("LoanSystem.Data.Entities.Ticket", "Ticket").WithMany("TicketResponses").HasForeignKey("TicketId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("CreatedBy");
            b.Navigation("Customer");
            b.Navigation("Ticket");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Transaction", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Agent").WithMany().HasForeignKey("AgentId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Contact", "Contact").WithMany().HasForeignKey("ContactId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.CustomerMarketerCommission", "CustomerMarketerCommission").WithMany().HasForeignKey("CustomerMarketerCommissionId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Loan", "SponsoredLoan").WithMany().HasForeignKey("SponsoredLoanId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.UserLoanContact", "UserLoanContact").WithMany().HasForeignKey("UserLoanContactId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Agent");
            b.Navigation("Contact");
            b.Navigation("Customer");
            b.Navigation("CustomerMarketerCommission");
            b.Navigation("SponsoredLoan");
            b.Navigation("UserLoan");
            b.Navigation("UserLoanContact");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Agent").WithMany().HasForeignKey("AgentId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId");
            b.HasOne("LoanSystem.Data.Entities.User", "CustomerMarketer").WithMany().HasForeignKey("CustomerMarketerId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Agent");
            b.Navigation("Customer");
            b.Navigation("CustomerMarketer");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Agent").WithMany().HasForeignKey("AgentId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.CustomerPurchasePlan", "CustomerPurchasePlan").WithMany().HasForeignKey("CustomerPurchasePlanId");
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany().HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", "SubAgent").WithMany().HasForeignKey("SubAgentId");
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoanRequest", "UserLoanRequest").WithMany().HasForeignKey("UserLoanRequestId")
                .OnDelete(DeleteBehavior.Restrict);
            b.Navigation("Agent");
            b.Navigation("CustomerPurchasePlan");
            b.Navigation("Loan");
            b.Navigation("SubAgent");
            b.Navigation("User");
            b.Navigation("UserLoanRequest");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanContact", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Contact", "Contact").WithMany("UserLoanContacts").HasForeignKey("ContactId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoan", null).WithMany("UserLoanContacts").HasForeignKey("UserLoanId1");
            b.Navigation("Contact");
            b.Navigation("Customer");
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPayment", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany("UserLoanPayments").HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPaymentRequest", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "AdminAcceptor").WithMany().HasForeignKey("AdminAcceptorId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.UserLoanPayment", "UserLoanPayment").WithMany("UserLoanPaymentRequests").HasForeignKey("UserLoanPaymentId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("AdminAcceptor");
            b.Navigation("UserLoanPayment");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequest", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Agent").WithMany().HasForeignKey("AgentId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "AgentStatus").WithMany().HasForeignKey("AgentStatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany().HasForeignKey("LoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "Status").WithMany().HasForeignKey("StatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Agent");
            b.Navigation("AgentStatus");
            b.Navigation("Loan");
            b.Navigation("Status");
            b.Navigation("User");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.AuthorizationItem", "AuthorizationItem").WithMany().HasForeignKey("AuthorizationItemId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoanRequest", "UserLoanRequest").WithMany("UserLoanRequestAuthorizationItems").HasForeignKey("UserLoanRequestId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.Navigation("AuthorizationItem");
            b.Navigation("UserLoanRequest");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItemAttachment", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItem", "UserLoanRequestAuthorizationItem").WithMany("Attachments").HasForeignKey("UserLoanRequestAuthorizationItemId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("UserLoanRequestAuthorizationItem");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestStatusHistory", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "FromAgentStatus").WithMany().HasForeignKey("FromAgentStatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "FromStatus").WithMany().HasForeignKey("FromStatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "ToAgentStatus").WithMany().HasForeignKey("ToAgentStatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.LoanRequestStatus", "ToStatus").WithMany().HasForeignKey("ToStatusId")
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoanRequest", "UserLoanRequest").WithMany().HasForeignKey("UserLoanRequestId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            b.Navigation("FromAgentStatus");
            b.Navigation("FromStatus");
            b.Navigation("ToAgentStatus");
            b.Navigation("ToStatus");
            b.Navigation("User");
            b.Navigation("UserLoanRequest");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanSponsoreDueDate", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.UserLoan", null).WithMany("UserLoanSponsoreDueDates").HasForeignKey("UserLoanId1");
            b.Navigation("Customer");
            b.Navigation("UserLoan");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserSavedMoney", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", "Member").WithMany().HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.Navigation("Member");
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<long>", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Role", null).WithMany().HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<long>", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", null).WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<long>", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", null).WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<long>", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.Role", null).WithMany().HasForeignKey("RoleId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.HasOne("LoanSystem.Data.Entities.User", null).WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<long>", (EntityTypeBuilder b) =>
        {
            b.HasOne("LoanSystem.Data.Entities.User", null).WithMany().HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Contact", (EntityTypeBuilder b) =>
        {
            b.Navigation("UserLoanContacts");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Customer", (EntityTypeBuilder b) =>
        {
            b.Navigation("Purchases");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
        {
            b.Navigation("AgentLoanAuthorizationItems");
            b.Navigation("LoanAuthorizationItems");
            b.Navigation("LoanContacts");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Purchase", (EntityTypeBuilder b) =>
        {
            b.Navigation("AffiliateCommissions");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Ticket", (EntityTypeBuilder b) =>
        {
            b.Navigation("TicketResponses");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.Transaction", (EntityTypeBuilder b) =>
        {
            b.Navigation("OnlineMoneyTransfers");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
        {
            b.Navigation("AgentLoans");
            b.Navigation("AgentMetaData");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
        {
            b.Navigation("Assurances");
            b.Navigation("UserLoanContacts");
            b.Navigation("UserLoanPayments");
            b.Navigation("UserLoanSponsoreDueDates");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPayment", (EntityTypeBuilder b) =>
        {
            b.Navigation("MemberPurchases");
            b.Navigation("UserLoanPaymentRequests");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequest", (EntityTypeBuilder b) =>
        {
            b.Navigation("UserLoanRequestAuthorizationItems");
        });
        modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanRequestAuthorizationItem", (EntityTypeBuilder b) =>
        {
            b.Navigation("Attachments");
        });
    }
}
