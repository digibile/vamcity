using System.ComponentModel.DataAnnotations;
using Ghestine.Domain.Common;
namespace Ghestine.Domain.Entities;

/// رکورد حسابرسی. هر رکورد هش رکورد قبلی را دارد (زنجیره)؛ تغییر یا حذف رکورد قدیمی با «بررسی زنجیره» پیدا می‌شود.
public class AuditEntry : BaseEntity
{
    public Guid TenantId { get; set; }
    public long Seq { get; set; }
    public Guid? UserId { get; set; }
    [MaxLength(120)] public string UserName { get; set; } = string.Empty;
    [MaxLength(10)] public string Method { get; set; } = string.Empty;
    [MaxLength(300)] public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    [MaxLength(45)] public string Ip { get; set; } = string.Empty;
    public DateTime At { get; set; } = DateTime.UtcNow;
    [MaxLength(64)] public string PrevHash { get; set; } = string.Empty;
    [MaxLength(64)] public string Hash { get; set; } = string.Empty;
}

/// سرفصل حسابداری. Type: Asset | Liability | Equity | Income | Expense
public class LedgerAccount : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(20)] public string Code { get; set; } = string.Empty;
    [MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(12)] public string Type { get; set; } = "Asset";
    public bool IsActive { get; set; } = true;
}

/// سند حسابداری دوطرفه. جمع بدهکار باید با جمع بستانکار برابر باشد. سند ثبت‌شده ویرایش نمی‌شود؛ فقط با سند معکوس اصلاح می‌شود.
public class JournalEntry : BaseEntity
{
    public Guid TenantId { get; set; }
    public long Number { get; set; }
    [MaxLength(300)] public string Memo { get; set; } = string.Empty;
    [MaxLength(60)] public string Reference { get; set; } = string.Empty;
    public Guid? ReversalOf { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class JournalLine : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid EntryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
