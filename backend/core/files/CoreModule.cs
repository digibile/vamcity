using System.ComponentModel.DataAnnotations;
using Ghestine.Domain.Common;
namespace Ghestine.Domain.Entities;

/// محصول وام (طرح اقساط). شرایط، پله‌ها و مدارک به‌صورت JSON ذخیره می‌شود تا بدون مایگریشن جدید قابل گسترش باشد.
public class LoanProduct : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(40)] public string Slug { get; set; } = string.Empty;
    [MaxLength(500)] public string Description { get; set; } = string.Empty;
    [MaxLength(40)] public string Badge { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int IntervalDays { get; set; } = 30;
    public string TiersJson { get; set; } = "[]";       // [{months,feePercent,downPaymentPercent}]
    public string ConditionsJson { get; set; } = "{}";  // {minAge,maxAge,minIncome,kyc,minScore,maxActive}
    public string DocumentsJson { get; set; } = "[]";   // [{name,required}]
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

/// کارت مجازی وام. شماره کامل و CVV فقط رمزنگاری‌شده (DataProtection) ذخیره می‌شود.
public class VirtualCard : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid LoanId { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(120)] public string HolderName { get; set; } = string.Empty;
    [MaxLength(8)] public string Last4 { get; set; } = string.Empty;
    [MaxLength(8)] public string Bin { get; set; } = "9101";
    [MaxLength(64)] public string PanHash { get; set; } = string.Empty;
    public string PanEnc { get; set; } = string.Empty;
    public string CvvEnc { get; set; } = string.Empty;
    public int ExpMonth { get; set; }
    public int ExpYear { get; set; }
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal PerTxnLimit { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Active"; // Active | Frozen | Closed
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class VirtualCardTxn : BaseEntity
{
    public Guid CardId { get; set; }
    [MaxLength(40)] public string Type { get; set; } = "Charge"; // Issue | Charge | Freeze | Unfreeze | Close | Reveal | LimitChange
    public decimal Amount { get; set; }
    [MaxLength(120)] public string Merchant { get; set; } = string.Empty;
    [MaxLength(80)] public string Reference { get; set; } = string.Empty;
    [MaxLength(200)] public string Note { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

/// زونکن دیجیتال: هر مدرک مشتری + متن استخراج‌شده (OCR) کنار فایل.
public class CustomerDocument : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? LoanId { get; set; }
    [MaxLength(80)] public string Kind { get; set; } = string.Empty; // کارت ملی، شناسنامه، ...
    [MaxLength(200)] public string FileName { get; set; } = string.Empty;
    [MaxLength(80)] public string ContentType { get; set; } = string.Empty;
    [MaxLength(300)] public string StoragePath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string? OcrText { get; set; }
    public string? FieldsJson { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "Pending"; // Pending | Verified | Rejected
    [MaxLength(300)] public string? ReviewNote { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
