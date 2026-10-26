using System.ComponentModel.DataAnnotations;
using Ghestine.Domain.Common;
namespace Ghestine.Domain.Entities;

/// هر ردیف یک کد یک‌بارمصرف. فقط هش کد ذخیره می‌شود و وضعیت زمان‌ها سمت سرور است.
public class OtpChallenge : BaseEntity
{
    [MaxLength(15)] public string Phone { get; set; } = string.Empty;
    [MaxLength(80)] public string CodeHash { get; set; } = string.Empty;
    [MaxLength(12)] public string Purpose { get; set; } = "login"; // login | register
    [MaxLength(45)] public string Ip { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime ResendAt { get; set; }
    public int Attempts { get; set; }
    public DateTime? ConsumedAt { get; set; }
    [MaxLength(20)] public string SendStatus { get; set; } = "Pending"; // Pending | Sent | Failed
    [MaxLength(200)] public string? SendInfo { get; set; }              // فقط کد وضعیت سرویس، نه متن کد
}
