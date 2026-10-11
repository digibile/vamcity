using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace LoanSystem.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20210513060951_UserLoanRequest")]
public class UserLoanRequest : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable("UserLoanRequests", (ColumnsBuilder table) => new
		{
			Id = table.Column<Guid>("uniqueidentifier"),
			UserId = table.Column<long>("bigint"),
			LoanId = table.Column<Guid>("uniqueidentifier"),
			RequestDate = table.Column<DateTime>("datetime2"),
			Amount = table.Column<decimal>("decimal(18,2)"),
			Comment = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
			AdminDescription = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
			Status = table.Column<int>("int"),
			Month = table.Column<int>("int"),
			Year = table.Column<int>("int")
		}, null, table =>
		{
			table.PrimaryKey("PK_UserLoanRequests", x => x.Id);
			table.ForeignKey("FK_UserLoanRequests_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
			table.ForeignKey("FK_UserLoanRequests_Loans_LoanId", x => x.LoanId, "Loans", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
		});
		migrationBuilder.CreateIndex("IX_UserLoanRequests_LoanId", "UserLoanRequests", "LoanId");
		migrationBuilder.CreateIndex("IX_UserLoanRequests_UserId", "UserLoanRequests", "UserId");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable("UserLoanRequests");
	}

	protected override void BuildTargetModel(ModelBuilder modelBuilder)
	{
		modelBuilder.UseIdentityColumns().HasAnnotation("Relational:MaxIdentifierLength", 128).HasAnnotation("ProductVersion", "5.0.2");
		modelBuilder.Entity("LoanSystem.Data.Entities.Expense", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<string>("Comment").HasColumnType("nvarchar(max)");
			b.Property<DateTime>("CostDate").HasColumnType("datetime2");
			b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
			b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
			b.Property<string>("Title").HasColumnType("nvarchar(max)");
			b.HasKey("Id");
			b.ToTable("Expenses");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<int>("Code").HasColumnType("int");
			b.Property<string>("Title").HasColumnType("nvarchar(max)");
			b.HasKey("Id");
			b.ToTable("Loans");
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
		modelBuilder.Entity("LoanSystem.Data.Entities.User", (EntityTypeBuilder b) =>
		{
			b.Property<long>("Id").ValueGeneratedOnAdd().HasColumnType("bigint")
				.UseIdentityColumn();
			b.Property<int>("AccessFailedCount").HasColumnType("int");
			b.Property<string>("Address").HasColumnType("nvarchar(max)");
			b.Property<string>("AgentCode").HasColumnType("nvarchar(max)");
			b.Property<string>("AgentTitle").HasColumnType("nvarchar(max)");
			b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("nvarchar(max)");
			b.Property<string>("Email").HasMaxLength(256).HasColumnType("nvarchar(256)");
			b.Property<bool>("EmailConfirmed").HasColumnType("bit");
			b.Property<string>("FirstName").HasColumnType("nvarchar(max)");
			b.Property<bool>("Gender").HasColumnType("bit");
			b.Property<string>("HoghoghiUnit").HasColumnType("nvarchar(max)");
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
			b.Property<string>("OfficeUnit").HasColumnType("nvarchar(max)");
			b.Property<string>("PasswordHash").HasColumnType("nvarchar(max)");
			b.Property<string>("PersonalCode").HasColumnType("nvarchar(max)");
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
			b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
			b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex")
				.HasFilter("[NormalizedUserName] IS NOT NULL");
			b.ToTable("AspNetUsers");
		});
		modelBuilder.Entity("LoanSystem.Data.Entities.UserLoan", (EntityTypeBuilder b) =>
		{
			b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
			b.Property<decimal>("Amount").HasColumnType("decimal(18,2)");
			b.Property<decimal>("Installment").HasColumnType("decimal(18,2)");
			b.Property<Guid>("LoanId").HasColumnType("uniqueidentifier");
			b.Property<int>("Month").HasColumnType("int");
			b.Property<decimal>("Paid").HasColumnType("decimal(18,2)");
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
			b.Property<string>("FileMonth").HasColumnType("nvarchar(max)");
			b.Property<int>("InstallmentNumber").HasColumnType("int");
			b.Property<DateTime>("SaveDate").HasColumnType("datetime2");
			b.Property<string>("SavedBy").HasColumnType("nvarchar(max)");
			b.Property<DateTime>("TargetMonth").HasColumnType("datetime2");
			b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
			b.HasKey("Id");
			b.HasIndex("UserLoanId");
			b.ToTable("UserLoanPayments");
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
			b.HasOne("LoanSystem.Data.Entities.UserLoan", "UserLoan").WithMany().HasForeignKey("UserLoanId")
				.OnDelete(DeleteBehavior.Cascade)
				.IsRequired();
			b.Navigation("UserLoan");
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
	}
}
