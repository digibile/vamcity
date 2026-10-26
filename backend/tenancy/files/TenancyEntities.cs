using System.ComponentModel.DataAnnotations;
using Ghestine.Domain.Common;
namespace Ghestine.Domain.Entities;

/// مستأجر (برند/سازمان). هر مستأجر دامنه‌ها، برند و داده‌های جدا دارد.
public class Tenant : BaseEntity
{
    [MaxLength(120)] public string Name { get; set; } = "دیجی‌بایل پی";
    [MaxLength(40)] public string Slug { get; set; } = "default";
    /// JSON: { "logoText":"د", "logoUrl":"", "primary":"#3b1f6b", "accent":"#f5a400", "support":"", "phone":"", "address":"" }
    public string BrandJson { get; set; } = "{}";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class TenantDomain : BaseEntity
{
    public Guid TenantId { get; set; }
    [MaxLength(253)] public string Host { get; set; } = string.Empty; // همیشه حروف کوچک، بدون پورت
    public bool IsPrimary { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
