using System;
using System.Collections.Generic;
using System.Linq;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoanSystem.Data;

public class DBInitializer
{
    public static void Initialize(AppDbContext context)
    {
        new DBInitializer().Seed(context);
    }

    public void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();
        if (context.Customers.Any())
        {
            return;
        }
        using IDbContextTransaction dbContextTransaction = context.Database.BeginTransaction();
        Role entity = new Role
        {
            Name = "SuperAdmin",
            NormalizedName = "SuperAdmin".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        Role entity2 = new Role
        {
            Name = "Marketer",
            NormalizedName = "Marketer".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        Role entity3 = new Role
        {
            Name = "CustomerMarketer",
            NormalizedName = "CustomerMarketer".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        Role role = new Role
        {
            Name = "Admin",
            NormalizedName = "Admin".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        Role entity4 = new Role
        {
            Name = "Member",
            NormalizedName = "Member".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        Role entity5 = new Role
        {
            Name = "Sponsor",
            NormalizedName = "Sponsor".ToUpper(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
        context.Set<Role>().Add(entity);
        context.Set<Role>().Add(entity2);
        context.Set<Role>().Add(entity3);
        context.Set<Role>().Add(role);
        context.Set<Role>().Add(entity4);
        context.Set<Role>().Add(entity5);
        Customer customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Default Customer",
            Email = "",
            Mobile = "",
            RegDate = DateTime.Now,
            ExpireDate = DateTime.Now.AddYears(1)
        };
        context.Set<Customer>().Add(customer);
        CustomerPurchasePlan entity6 = new CustomerPurchasePlan
        {
            CustomerId = customer.Id,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddYears(1),
            MaxLoanCount = 1000000,
            RemainLoanCount = 1000000
        };
        context.Set<CustomerPurchasePlan>().Add(entity6);
        context.SaveChanges();
        User user = new User
        {
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SecurityStamp = "5440e31d-b341-4046-b8f2-628f3ef54747",
            UserName = "Admin",
            NormalizedUserName = "Admin".Normalize().ToUpperInvariant(),
            FirstName = "کاربر ",
            LastName = "مدیر",
            IsActive = true,
            CustomerId = customer.Id
        };
        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        string passwordHash = passwordHasher.HashPassword(user, "PASSWORD");
        user.PasswordHash = passwordHash;
        User user2 = new User
        {
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            SecurityStamp = "BD99C219-0F2E-4F8B-926F-08DB1EF5FC64",
            UserName = "SuperAdmin",
            NormalizedUserName = "SuperAdmin".Normalize().ToUpperInvariant(),
            FirstName = "علی",
            LastName = "حورشناس",
            IsActive = true
        };
        passwordHash = passwordHasher.HashPassword(user2, "!@#123QWEqwe");
        user2.PasswordHash = passwordHash;
        context.Set<User>().Add(user);
        context.Set<User>().Add(user2);
        context.SaveChanges();
        context.UserRoles.AddRange(new IdentityUserRole<long>
        {
            RoleId = role.Id,
            UserId = user.Id
        });
        foreach (object value in Enum.GetValues(typeof(UserPermissionClaims)))
        {
            context.UserClaims.Add(new IdentityUserClaim<long>
            {
                UserId = user.Id,
                ClaimType = "Permission",
                ClaimValue = value.ToString()
            });
        }
        SmsPattern entity7 = new SmsPattern
        {
            CustomerId = customer.Id,
            IsSystem = true,
            Pattern = "مشتری گرامی {نام مشتری}؛ سررسید قسط به مبلغ {مبلغ قسط} فرا رسیده است، لطفا نسبت به پرداخت به موقع اقدام فرمایید.\r\n {نام فروشگاه}",
            Title = "پیامک یادآور سررسید",
            Status = SmsPatternStatus.Approved,
            Category = SmsPatternCategory.قسط,
            IsPatternBase = false,
            SmsPatternPanel = SmsPanel.MelliPayamak,
            PatternBaseCode = "356784"
        };
        SmsPattern entity8 = new SmsPattern
        {
            CustomerId = customer.Id,
            IsSystem = true,
            Pattern = "مشتری گرامی {نام مشتری}؛ سررسید قسط به مبلغ {مبلغ مانده قسط} معوق شده است، لطفا در اسرع وقت نسبت به پرداخت اقدام فرمایید.\r\n {نام فروشگاه}",
            Title = "پیامک یادآور قسط معوق",
            Status = SmsPatternStatus.Approved,
            Category = SmsPatternCategory.قسط,
            IsPatternBase = false,
            SmsPatternPanel = SmsPanel.MelliPayamak,
            PatternBaseCode = "356783"
        };
        SmsPattern entity9 = new SmsPattern
        {
            CustomerId = customer.Id,
            IsSystem = true,
            Pattern = "قسط به مبلغ {مبلغ آخرین واریز} دریافت شد. مانده وام {مانده وام}.\r\nبا تشکر {نام فروشگاه}",
            Title = "تشکر از پرداخت قسط",
            Status = SmsPatternStatus.Approved,
            Category = SmsPatternCategory.قسط,
            IsPatternBase = false,
            SmsPatternPanel = SmsPanel.MelliPayamak,
            PatternBaseCode = "356782"
        };
        SmsPattern entity10 = new SmsPattern
        {
            CustomerId = customer.Id,
            IsSystem = true,
            Pattern = "درود {نام مشتری} گرامی؛ سررسید قسط شماره {شماره قسط} به مبلغ {مبلغ قسط} طی 2 روز آینده فرا خواهد رسید، لطفا نسبت به پرداخت به موقع اقدام فرمایید.",
            Title = "یادآوری پیش از موعد",
            Status = SmsPatternStatus.Approved,
            Category = SmsPatternCategory.قسط,
            IsPatternBase = false,
            SmsPatternPanel = SmsPanel.MelliPayamak,
            PatternBaseCode = "356549"
        };
        context.SmsPatterns.Add(entity7);
        context.SmsPatterns.Add(entity8);
        context.SmsPatterns.Add(entity9);
        context.SmsPatterns.Add(entity10);
        List<LoanRequestStatus> entities = new List<LoanRequestStatus>
        {
            new LoanRequestStatus
            {
                Title = "ایجاد اولیه",
                StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                Changable = true,
                Order = 0,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.ایجاد_اولیه
            },
            new LoanRequestStatus
            {
                Title = "تکمیل مدارک",
                StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                Changable = true,
                Order = 2,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.تکمیل_مدارک
            },
            new LoanRequestStatus
            {
                Title = "مدارک ناقص تکمیل مدارک",
                StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                Changable = true,
                Order = 4,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.مدارک_ناقص_تکمیل_مدارک
            },
            new LoanRequestStatus
            {
                Title = "در انتظار بررسی کارشناس",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 6,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.در_انتظار_بررسی_کارشناس
            },
            new LoanRequestStatus
            {
                Title = "نیاز به پیگیری",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 8,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.نیاز_به_پیگیری
            },
            new LoanRequestStatus
            {
                Title = "بررسی مجدد کارشناس",
                StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                Changable = false,
                Order = 9,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.بررسی_مجدد_کارشناس
            },
            new LoanRequestStatus
            {
                Title = "رد شده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 10,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.رد_شده,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "اولویت بندی شده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 12,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.اولویت_بندی_شده
            },
            new LoanRequestStatus
            {
                Title = "در انتظار تسویه با نماینده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 13,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.در_انتظار_تسویه_با_نماینده,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "پرداخت شده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 14,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.پرداخت_شده,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "بتا ثبت شده انتظار بررسی کارشناس",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                Changable = false,
                Order = 16,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemRequestStatus = SystemRequestStatus.رفاه_بتا_ثبت_شد_انتظار_بررسی_کارشناس,
                AllowNewRequest = true
            }
        };
        context.AddRange(entities);
        List<LoanRequestStatus> entities2 = new List<LoanRequestStatus>
        {
            new LoanRequestStatus
            {
                Title = "بدون وضعیت",
                StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 0,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.بدون_وضعیت,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "در انتظار بررسی",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 2,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.در_انتظار_بررسی
            },
            new LoanRequestStatus
            {
                Title = "نقص مدارک",
                StepCategory = LoanRequestStatusCategorys.تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 4,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.نقص_مدارک
            },
            new LoanRequestStatus
            {
                Title = "ارسال به تامین کننده",
                StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 6,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.ارسال_به_تامین_کننده
            },
            new LoanRequestStatus
            {
                Title = "اعطا شده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 8,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.اعطاـشده,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "در انتظار تسویه",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 10,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.در_انتظار_تسویه,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "تسویه شده",
                StepCategory = LoanRequestStatusCategorys.قبل_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 12,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.تسویه_شده,
                AllowNewRequest = true
            },
            new LoanRequestStatus
            {
                Title = "رد شده",
                StepCategory = LoanRequestStatusCategorys.بعد_از_تکمیل_مدارک,
                StatusGroup = StatusGroup.نماینده,
                Changable = true,
                Order = 14,
                CustomerId = customer.Id,
                SmsPatternId = null,
                SystemAgentRequestStatus = SystemAgentRequestStatus.رد_شده,
                AllowNewRequest = true
            }
        };
        context.AddRange(entities2);
        context.SaveChanges();
        dbContextTransaction.Commit();
    }
}
