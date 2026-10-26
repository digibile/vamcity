using System.ComponentModel.DataAnnotations;
using Ghestine.Domain.Common;
namespace Ghestine.Domain.Entities;

/// اعلام واریز مشتری (کارت‌به‌کارت یا واریز بانکی). تأیید این رکورد فقط وضعیت آن را عوض می‌کند؛ ثبت در اقساط وام جداگانه و بعد از دیدن ساختار وام انجام می‌شود.
public class PaymentDeclaration : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? LoanId { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(30)] public string Method { get; set; } = "CardToCard"; // CardToCard | BankTransfer
    [MaxLength(60)] public string Reference { get; set; } = string.Empty; // شماره پیگیری
    public DateTime PaidAt { get; set; }
    [MaxLength(300)] public string Note { get; set; } = string.Empty;
    [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
    [MaxLength(300)] public string? ReviewNote { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

/// حساب بانکی طرف تسویه (پذیرنده، تأمین‌کننده).
public class CounterParty : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(30)] public string Iban { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class Marketer : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(15)] public string Phone { get; set; } = string.Empty;
    [MaxLength(20)] public string Code { get; set; } = string.Empty;   // کد معرفی یکتا در هر مستأجر
    public decimal CommissionPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

/// تنظیمات کلید-مقدار هر مستأجر (مثلاً نسخه‌ی اپ).
public class AppSetting : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(60)] public string Key { get; set; } = string.Empty;
    [MaxLength(500)] public string Value { get; set; } = string.Empty;
}

/// درخواست استعلام (اعتبار/هویت) برای یک مشتری. نتیجه را سرویس‌دهنده یا مدیر ثبت می‌کند.
public class InquiryRequest : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(40)] public string Type { get; set; } = "Credit";   // Credit | Identity | Other
    [MaxLength(20)] public string Status { get; set; } = "Queued"; // Queued | Done | Failed
    [MaxLength(300)] public string Result { get; set; } = string.Empty;
    [MaxLength(300)] public string Note { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? DoneAt { get; set; }
}

/// سفارش خرید مشتری از یک پذیرنده با وام.
public class PurchaseOrder : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? LoanId { get; set; }
    [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(120)] public string MerchantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending | Delivered | Cancelled
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
