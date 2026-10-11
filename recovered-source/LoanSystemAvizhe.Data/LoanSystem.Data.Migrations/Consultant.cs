using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace LoanSystem.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20230523081137_Consultant")]
public class Consultant : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable("Consultant", (ColumnsBuilder table) =>
		{
			OperationBuilder<AddColumnOperation> id = table.Column<Guid>("uniqueidentifier");
			int? maxLength = 100;
			OperationBuilder<AddColumnOperation> name = table.Column<string>("nvarchar(100)", null, maxLength, rowVersion: false, null, nullable: true);
			maxLength = 20;
			OperationBuilder<AddColumnOperation> phone = table.Column<string>("nvarchar(20)", null, maxLength, rowVersion: false, null, nullable: true);
			maxLength = 1000;
			OperationBuilder<AddColumnOperation> description = table.Column<string>("nvarchar(1000)", null, maxLength, rowVersion: false, null, nullable: true);
			OperationBuilder<AddColumnOperation> isSeen = table.Column<bool>("bit");
			maxLength = 1000;
			return new
			{
				Id = id,
				Name = name,
				Phone = phone,
				Description = description,
				IsSeen = isSeen,
				CallResult = table.Column<string>("nvarchar(1000)", null, maxLength, rowVersion: false, null, nullable: true),
				SaveDate = table.Column<DateTime>("datetime2")
			};
		}, null, table =>
		{
			table.PrimaryKey("PK_Consultant", x => x.Id);
		});
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable("Consultant");
	}

	public static string ScriptToPrevious()
	{
		return "DROP TABLE [Consultant];\r\n\r\nDELETE FROM [__EFMigrationsHistory]\r\nWHERE [MigrationId] = N'20230523081137_Consultant';";
	}

	protected override void BuildTargetModel(ModelBuilder modelBuilder)
	{
		modelBuilder.UseIdentityColumns().HasAnnotation("Relational:MaxIdentifierLength", 128).HasAnnotation("ProductVersion", "5.0.2");
		modelBuilder.Entity("LoanSystem.Data.Entities.AppSettings", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<int>("AppendToWhichInstallment").HasColumnType("int");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<Guid?>("DueDateRememberSmsPatternId").HasColumnType("uniqueidentifier");
			b.Property<TimeSpan?>("DueDatedRememberSmsTime").HasColumnType("time");
			b.Property<bool>("IsOnlinePaymentActive").HasColumnType("bit");
			b.Property<int>("OnlinePaymentGateway").HasColumnType("int");
			b.Property<byte>("RoundCountNumber").HasColumnType("tinyint");
			b.Property<string>("Shaba").HasMaxLength(50).HasColumnType("nvarchar(50)");
			b.Property<int>("SmsCredit").HasColumnType("int");
			b.Property<Guid?>("ThanksPaymentSmsPatternId").HasColumnType("uniqueidentifier");
			b.Property<string>("ZarinpalMerchantCode").HasMaxLength(200).HasColumnType("nvarchar(200)");
			b.HasKey("Id");
			b.HasIndex("CustomerId");
			b.HasIndex("DueDateRememberSmsPatternId");
			b.HasIndex("ThanksPaymentSmsPatternId");
			b.ToTable("AppSettings");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.AppSms", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<int>("AppSmsType").HasColumnType("int");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<DateTime>("Date").HasColumnType("datetime2");
			b.Property<decimal>("Price").HasColumnType("decimal(18,2)");
			b.Property<int>("Status").HasColumnType("int");
			b.Property<string>("Text").HasMaxLength(200).HasColumnType("nvarchar(200)");
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
			b.ToTable("Consultant");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.Customer", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
				.UseIdentityColumn();
			b.Property<string>("Email").HasColumnType("nvarchar(max)");
			b.Property<DateTime>("ExpireDate").HasColumnType("datetime2");
			b.Property<bool>("FirstLoginDone").HasColumnType("bit");
			b.Property<string>("InitPassword").HasColumnType("nvarchar(max)");
			b.Property<string>("Mobile").HasColumnType("nvarchar(max)");
			b.Property<string>("Name").HasColumnType("nvarchar(max)");
			b.Property<bool>("Permanent").HasColumnType("bit");
			b.Property<bool>("Purchased").HasColumnType("bit");
			b.Property<DateTime>("RegDate").HasColumnType("datetime2");
			b.HasKey("Id");
			b.ToTable("Customers");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<int>("Code").HasColumnType("int");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<double>("DelayPercent").HasColumnType("float");
			b.Property<double>("EarlyPercent").HasColumnType("float");
			b.Property<bool>("IsSponsored").HasColumnType("bit");
			b.Property<int>("MaxInstallments").HasColumnType("int");
			b.Property<double>("ProfitPercent").HasColumnType("float");
			b.Property<string>("Title").HasColumnType("nvarchar(max)");
			b.HasKey("Id");
			b.HasIndex("CustomerId");
			b.ToTable("Loans");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.MemberPurchase", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<string>("Description").HasMaxLength(200).HasColumnType("nvarchar(200)");
			b.Property<bool>("IsPaymentSuccess").HasColumnType("bit");
			b.Property<int>("OnlinePaymentGateway").HasColumnType("int");
			b.Property<int>("PaymentStep").HasColumnType("int");
			b.Property<DateTime>("StartDate").HasColumnType("datetime2");
			b.Property<long>("UserId").HasColumnType("bigint");
			b.Property<Guid>("UserLoanPaymentId").HasColumnType("uniqueidentifier");
			b.Property<string>("ZarinPalAuthority").HasMaxLength(50).HasColumnType("nvarchar(50)");
			b.Property<string>("ZarinPalMerchantId").HasColumnType("nvarchar(max)");
			b.Property<string>("ZarinPalRefId").HasMaxLength(200).HasColumnType("nvarchar(200)");
			b.Property<int>("ZarinPalStatus").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("UserId");
			b.HasIndex("UserLoanPaymentId");
			b.ToTable("MemberPurchases");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.Purchase", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<string>("Description").HasMaxLength(200).HasColumnType("nvarchar(200)");
			b.Property<bool>("IsPaymentSuccess").HasColumnType("bit");
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
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<bool>("IsSystem").HasColumnType("bit");
			b.Property<string>("Pattern").HasColumnType("nvarchar(max)");
			b.Property<string>("RejectReason").HasColumnType("nvarchar(max)");
			b.Property<int>("Status").HasColumnType("int");
			b.Property<string>("Title").HasMaxLength(200).HasColumnType("nvarchar(200)");
			b.HasKey("Id");
			b.HasIndex("CustomerId");
			b.ToTable("SmsPatterns");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.Transaction", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
			b.Property<string>("Description").HasColumnType("nvarchar(max)");
			b.Property<double>("GoldRate").HasColumnType("float");
			b.Property<double>("GoldWeight").HasColumnType("float");
			b.Property<bool>("IsUnknown").HasColumnType("bit");
			b.Property<bool>("IsWithdraw").HasColumnType("bit");
			b.Property<int>("PaymentMethod").HasColumnType("int");
			b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
			b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
			b.Property<Guid?>("SponsoredLoanId").HasColumnType("uniqueidentifier");
			b.Property<double>("WeightBalance").HasColumnType("float");
			b.HasKey("Id");
			b.HasIndex("CustomerId");
			b.HasIndex("SponsoredLoanId");
			b.ToTable("Withdraws");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
		{
			b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint")
				.UseIdentityColumn();
			b.Property<int>("AccessFailedCount").HasColumnType("int");
			b.Property<string>("Address").HasColumnType("nvarchar(max)");
			b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
			b.Property<Guid>("CustomerId").HasColumnType("uniqueidentifier");
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
			b.Property<string>("NationalCode").HasColumnType("nvarchar(max)");
			b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("nvarchar(256)");
			b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
			b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
			b.Property<string>("PhoneNumber").HasColumnType("nvarchar(max)");
			b.Property<bool>("PhoneNumberConfirmed").HasColumnType("bit");
			b.Property<DateTime>("RegDate").HasColumnType("datetime2");
			b.Property<DateTime>("RegDate2").HasColumnType("datetime2");
			b.Property<string>("SecurityStamp").HasColumnType("nvarchar(max)");
			b.Property<decimal>("TotalSavedMoney").HasColumnType("decimal(18,2)");
			b.Property<bool>("TwoFactorEnabled").HasColumnType("bit");
			b.Property<string>("UserName").HasMaxLength(256).HasColumnType("nvarchar(256)");
			b.Property<int>("ViewCount").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("CustomerId");
			b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
			b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex")
				.HasFilter("[NormalizedUserName] IS NOT NULL");
			b.ToTable("AspNetUsers");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<DateTime?>("CheckoutDate").HasColumnType("datetime2");
			b.Property<int>("Code").ValueGeneratedOnAdd().HasColumnType("int")
				.UseIdentityColumn();
			b.Property<string>("Description").HasColumnType("nvarchar(max)");
			b.Property<decimal>("Installment").HasColumnType("decimal(18,2)");
			b.Property<bool?>("IsCheckouted").HasColumnType("bit");
			b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
			b.Property<int>("Month").HasColumnType("int");
			b.Property<int>("MonthGap").ValueGeneratedOnAdd().HasColumnType("int")
				.HasDefaultValue(1);
			b.Property<decimal>("Paid").HasColumnType("decimal(18,2)");
			b.Property<decimal>("PayableAmount").HasColumnType("decimal(18,2)");
			b.Property<decimal>("Remain").HasColumnType("decimal(18,2)");
			b.Property<DateTime>("StartDate").HasColumnType("datetime2");
			b.Property<long>("UserId").HasColumnType("bigint");
			b.HasKey("Id");
			b.HasIndex("LoanId");
			b.HasIndex("UserId");
			b.ToTable("UserLoans");
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
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<string>("Comment").HasColumnType("nvarchar(max)");
			b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
			b.Property<int>("Month").HasColumnType("int");
			b.Property<DateTime>("RequestDate").HasColumnType("datetime2");
			b.Property<int>("Status").HasColumnType("int");
			b.Property<long>("UserId").HasColumnType("bigint");
			b.Property<int>("Year").HasColumnType("int");
			b.HasKey("Id");
			b.HasIndex("LoanId");
			b.HasIndex("UserId");
			b.ToTable("UserLoanRequests");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.AppSettings", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("LoanSystem.Data.Entities.SmsPattern", "DueDateRememberSmsPattern").WithMany().HasForeignKey("DueDateRememberSmsPatternId");
			b.HasOne("LoanSystem.Data.Entities.SmsPattern", "ThanksPaymentSmsPattern").WithMany().HasForeignKey("ThanksPaymentSmsPatternId");
			b.Navigation("Customer");
			b.Navigation("DueDateRememberSmsPattern");
			b.Navigation("ThanksPaymentSmsPattern");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.Expense", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Customer");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Customer");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.MemberPurchase", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("LoanSystem.Data.Entities.UserLoanPayment", "UserLoanPayment").WithMany("MemberPurchases").HasForeignKey("UserLoanPaymentId")
				.OnDelete(DeleteBehavior.NoAction)
				.IsRequired();
			b.Navigation("User");
			b.Navigation("UserLoanPayment");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.Transaction", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("LoanSystem.Data.Entities.Loan", "SponsoredLoan").WithMany().HasForeignKey("SponsoredLoanId");
			b.Navigation("Customer");
			b.Navigation("SponsoredLoan");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Customer", "Customer").WithMany().HasForeignKey("CustomerId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Customer");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany().HasForeignKey("LoanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Loan");
			b.Navigation("User");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPayment", (EntityTypeBuilder b) =>
		{
			b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany("UserLoanPayments").HasForeignKey("UserLoanId")
				.OnDelete(DeleteBehavior.Cascade)
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
			b.HasOne("LoanSystem.Data.Entities.Loan", "Loan").WithMany().HasForeignKey("LoanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.HasOne("LoanSystem.Data.Entities.User", "User").WithMany().HasForeignKey("UserId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("Loan");
			b.Navigation("User");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.Customer", (EntityTypeBuilder b) =>
		{
			b.Navigation("Purchases");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
		{
			b.Navigation("Assurances");
			b.Navigation("UserLoanPayments");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoanPayment", (EntityTypeBuilder b) =>
		{
			b.Navigation("MemberPurchases");
			b.Navigation("UserLoanPaymentRequests");
		});
	}
}
