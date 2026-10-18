using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace LoanSystem.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20210302100342_Init")]
public class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("AspNetRoles", (ColumnsBuilder table) =>
        {
            OperationBuilder<AddColumnOperation> id = table.Column<long>("bigint").Annotation("SqlServer:Identity", "1, 1");
            int? maxLength = 256;
            OperationBuilder<AddColumnOperation> name = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true);
            maxLength = 256;
            return new
            {
                Id = id,
                Name = name,
                NormalizedName = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true),
                ConcurrencyStamp = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
            };
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetRoles", x => x.Id);
        });
        migrationBuilder.CreateTable("AspNetUsers", (ColumnsBuilder table) =>
        {
            OperationBuilder<AddColumnOperation> id = table.Column<long>("bigint").Annotation("SqlServer:Identity", "1, 1");
            OperationBuilder<AddColumnOperation> firstName = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> lastName = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> gender = table.Column<bool>("bit");
            OperationBuilder<AddColumnOperation> address = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> mobile = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> lastLoginDate = table.Column<DateTime>("datetime2");
            OperationBuilder<AddColumnOperation> isActive = table.Column<bool>("bit");
            OperationBuilder<AddColumnOperation> personalCode = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> nationalCode = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> agentCode = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> agentTitle = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> hoghoghiUnit = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> officeUnit = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true);
            OperationBuilder<AddColumnOperation> totalSavedMoney = table.Column<decimal>("decimal(18,2)");
            OperationBuilder<AddColumnOperation> viewCount = table.Column<int>("int");
            int? maxLength = 256;
            OperationBuilder<AddColumnOperation> userName = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true);
            maxLength = 256;
            OperationBuilder<AddColumnOperation> normalizedUserName = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true);
            maxLength = 256;
            OperationBuilder<AddColumnOperation> email = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true);
            maxLength = 256;
            return new
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Gender = gender,
                Address = address,
                Mobile = mobile,
                LastLoginDate = lastLoginDate,
                IsActive = isActive,
                PersonalCode = personalCode,
                NationalCode = nationalCode,
                AgentCode = agentCode,
                AgentTitle = agentTitle,
                HoghoghiUnit = hoghoghiUnit,
                OfficeUnit = officeUnit,
                TotalSavedMoney = totalSavedMoney,
                ViewCount = viewCount,
                UserName = userName,
                NormalizedUserName = normalizedUserName,
                Email = email,
                NormalizedEmail = table.Column<string>("nvarchar(256)", null, maxLength, rowVersion: false, null, nullable: true),
                EmailConfirmed = table.Column<bool>("bit"),
                PasswordHash = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
                SecurityStamp = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
                ConcurrencyStamp = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
                PhoneNumber = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
                PhoneNumberConfirmed = table.Column<bool>("bit"),
                TwoFactorEnabled = table.Column<bool>("bit"),
                LockoutEnd = table.Column<DateTimeOffset>("datetimeoffset", null, null, rowVersion: false, null, nullable: true),
                LockoutEnabled = table.Column<bool>("bit"),
                AccessFailedCount = table.Column<int>("int")
            };
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetUsers", x => x.Id);
        });
        migrationBuilder.CreateTable("Loans", (ColumnsBuilder table) => new
        {
            Id = table.Column<Guid>("uniqueidentifier"),
            Title = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_Loans", x => x.Id);
        });
        migrationBuilder.CreateTable("AspNetRoleClaims", (ColumnsBuilder table) => new
        {
            Id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1"),
            RoleId = table.Column<long>("bigint"),
            ClaimType = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            ClaimValue = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("AspNetUserClaims", (ColumnsBuilder table) => new
        {
            Id = table.Column<int>("int").Annotation("SqlServer:Identity", "1, 1"),
            UserId = table.Column<long>("bigint"),
            ClaimType = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            ClaimValue = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            table.ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("AspNetUserLogins", (ColumnsBuilder table) => new
        {
            LoginProvider = table.Column<string>("nvarchar(450)"),
            ProviderKey = table.Column<string>("nvarchar(450)"),
            ProviderDisplayName = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            UserId = table.Column<long>("bigint")
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            table.ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("AspNetUserRoles", (ColumnsBuilder table) => new
        {
            UserId = table.Column<long>("bigint"),
            RoleId = table.Column<long>("bigint")
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
            table.ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("AspNetUserTokens", (ColumnsBuilder table) => new
        {
            UserId = table.Column<long>("bigint"),
            LoginProvider = table.Column<string>("nvarchar(450)"),
            Name = table.Column<string>("nvarchar(450)"),
            Value = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            table.ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("UserSavedMonies", (ColumnsBuilder table) => new
        {
            Id = table.Column<Guid>("uniqueidentifier"),
            MemberId = table.Column<long>("bigint"),
            Description = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            Amount = table.Column<decimal>("decimal(18,2)"),
            SavedBy = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            SaveDate = table.Column<DateTime>("datetime2"),
            FileMonth = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_UserSavedMonies", x => x.Id);
            table.ForeignKey("FK_UserSavedMonies_AspNetUsers_MemberId", x => x.MemberId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("UserLoans", (ColumnsBuilder table) => new
        {
            Id = table.Column<Guid>("uniqueidentifier"),
            UserId = table.Column<long>("bigint"),
            LoanId = table.Column<Guid>("uniqueidentifier"),
            StartDate = table.Column<DateTime>("datetime2"),
            Amount = table.Column<decimal>("decimal(18,2)"),
            Installment = table.Column<decimal>("decimal(18,2)"),
            Month = table.Column<int>("int"),
            Paid = table.Column<decimal>("decimal(18,2)"),
            Remain = table.Column<decimal>("decimal(18,2)")
        }, null, table =>
        {
            table.PrimaryKey("PK_UserLoans", x => x.Id);
            table.ForeignKey("FK_UserLoans_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
            table.ForeignKey("FK_UserLoans_Loans_LoanId", x => x.LoanId, "Loans", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("UserLoanPayments", (ColumnsBuilder table) => new
        {
            Id = table.Column<Guid>("uniqueidentifier"),
            UserLoanId = table.Column<Guid>("uniqueidentifier"),
            InstallmentNumber = table.Column<int>("int"),
            Amount = table.Column<decimal>("decimal(18,2)"),
            SavedBy = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true),
            SaveDate = table.Column<DateTime>("datetime2"),
            FileMonth = table.Column<string>("nvarchar(max)", null, null, rowVersion: false, null, nullable: true)
        }, null, table =>
        {
            table.PrimaryKey("PK_UserLoanPayments", x => x.Id);
            table.ForeignKey("FK_UserLoanPayments_UserLoans_UserLoanId", x => x.UserLoanId, "UserLoans", "Id", null, ReferentialAction.NoAction, ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_AspNetRoleClaims_RoleId", "AspNetRoleClaims", "RoleId");
        migrationBuilder.CreateIndex("RoleNameIndex", "AspNetRoles", "NormalizedName", null, unique: true, "[NormalizedName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_AspNetUserClaims_UserId", "AspNetUserClaims", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserLogins_UserId", "AspNetUserLogins", "UserId");
        migrationBuilder.CreateIndex("IX_AspNetUserRoles_RoleId", "AspNetUserRoles", "RoleId");
        migrationBuilder.CreateIndex("EmailIndex", "AspNetUsers", "NormalizedEmail");
        migrationBuilder.CreateIndex("UserNameIndex", "AspNetUsers", "NormalizedUserName", null, unique: true, "[NormalizedUserName] IS NOT NULL");
        migrationBuilder.CreateIndex("IX_UserLoanPayments_UserLoanId", "UserLoanPayments", "UserLoanId");
        migrationBuilder.CreateIndex("IX_UserLoans_LoanId", "UserLoans", "LoanId");
        migrationBuilder.CreateIndex("IX_UserLoans_UserId", "UserLoans", "UserId");
        migrationBuilder.CreateIndex("IX_UserSavedMonies_MemberId", "UserSavedMonies", "MemberId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AspNetRoleClaims");
        migrationBuilder.DropTable("AspNetUserClaims");
        migrationBuilder.DropTable("AspNetUserLogins");
        migrationBuilder.DropTable("AspNetUserRoles");
        migrationBuilder.DropTable("AspNetUserTokens");
        migrationBuilder.DropTable("UserLoanPayments");
        migrationBuilder.DropTable("UserSavedMonies");
        migrationBuilder.DropTable("AspNetRoles");
        migrationBuilder.DropTable("UserLoans");
        migrationBuilder.DropTable("AspNetUsers");
        migrationBuilder.DropTable("Loans");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.UseIdentityColumns().HasAnnotation("Relational:MaxIdentifierLength", 128).HasAnnotation("ProductVersion", "5.0.2");
        modelBuilder.Entity("LoanSystem.Data.Entities.Loan", (EntityTypeBuilder b) =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
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
            b.Property<Guid>("UserLoanId").HasColumnType("uniqueidentifier");
            b.HasKey("Id");
            b.HasIndex("UserLoanId");
            b.ToTable("UserLoanPayments");
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
