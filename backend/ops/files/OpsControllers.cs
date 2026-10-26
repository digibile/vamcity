using System.Numerics;
using System.Security.Claims;
using Ghestine.Domain.Entities;
using Ghestine.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using __DBNS__;
namespace Ghestine.API.Ops;

static class OpsU
{
    public static Guid? Uid(this ControllerBase c) => Guid.TryParse(c.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? c.User.FindFirstValue("sub"), out var g) ? g : null;
    public static string Digits(string? s) => new string((s ?? "").Select(ch => ch >= '۰' && ch <= '۹' ? (char)('0' + ch - '۰') : ch).Where(char.IsDigit).ToArray());
    /// شماره شبا: IR + ۲۴ رقم، با کنترل MOD-97.
    public static bool IbanOk(string iban)
    {
        iban = (iban ?? "").Replace(" ", "").ToUpperInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(iban, "^IR[0-9]{24}$")) return false;
        var r = iban[4..] + iban[..4]; var sb = new System.Text.StringBuilder();
        foreach (var ch in r) sb.Append(char.IsLetter(ch) ? (ch - 'A' + 10).ToString() : ch.ToString());
        return BigInteger.Parse(sb.ToString()) % 97 == 1;
    }
}

// ===== اعلام واریز: مشتری =====
public record DeclareInput(decimal Amount, string Method, string Reference, DateTime PaidAt, Guid? LoanId, string? Note);
[ApiController, Route("api/v1/me/payment-declarations"), Authorize]
public class MyDeclarationsController : ControllerBase
{
    private readonly AppDbContext _db; public MyDeclarationsController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> Mine(CancellationToken ct)
    { var u = this.Uid(); if (u == null) return Unauthorized(); return Ok(await _db.Set<PaymentDeclaration>().Where(d => d.TenantId == this.Tid() && d.UserId == u).OrderByDescending(d => d.CreatedOn).Take(100).Select(d => new { d.Id, d.Amount, d.Method, d.Reference, d.PaidAt, d.Status, d.ReviewNote, d.CreatedOn }).ToListAsync(ct)); }
    [HttpPost] public async Task<IActionResult> Create(DeclareInput i, CancellationToken ct)
    {
        var u = this.Uid(); if (u == null) return Unauthorized();
        if (i.Amount <= 0 || i.Amount > 10_000_000_000m) return BadRequest(new { message = "مبلغ نامعتبر است" });
        if (i.Method is not ("CardToCard" or "BankTransfer")) return BadRequest(new { message = "روش پرداخت نامعتبر است" });
        var rf = OpsU.Digits(i.Reference); if (rf.Length < 4 || rf.Length > 30) return BadRequest(new { message = "شماره پیگیری را درست وارد کنید" });
        if (i.PaidAt > DateTime.UtcNow.AddDays(1) || i.PaidAt < DateTime.UtcNow.AddYears(-1)) return BadRequest(new { message = "تاریخ پرداخت نامعتبر است" });
        if (await _db.Set<PaymentDeclaration>().AnyAsync(d => d.TenantId == this.Tid() && d.Reference == rf && d.Amount == i.Amount && d.Status != "Rejected", ct)) return Conflict(new { message = "این واریز قبلاً ثبت شده است" });
        if (await _db.Set<PaymentDeclaration>().CountAsync(d => d.UserId == u && d.Status == "Pending", ct) >= 10) return StatusCode(429, new { message = "ابتدا منتظر بررسی واریزهای قبلی بمانید" });
        var d = new PaymentDeclaration { TenantId = this.Tid(), UserId = u.Value, LoanId = i.LoanId, Amount = i.Amount, Method = i.Method, Reference = rf, PaidAt = i.PaidAt, Note = (i.Note ?? "").Trim()[..Math.Min(300, (i.Note ?? "").Trim().Length)] };
        _db.Add(d); await _db.SaveChangesAsync(ct); return Ok(new { d.Id, d.Status });
    }
}

// ===== اعلام واریز: مدیر =====
public record ReviewDecl(string Status, string? Note);
[ApiController, Route("api/v1/admin/payment-declarations"), Authorize(Roles = "Admin,SuperAdmin")]
public class DeclarationsAdminController : ControllerBase
{
    private readonly AppDbContext _db; public DeclarationsAdminController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        var q = _db.Set<PaymentDeclaration>().Where(d => d.TenantId == this.Tid());
        if (!string.IsNullOrEmpty(status)) q = q.Where(d => d.Status == status);
        return Ok(await q.OrderByDescending(d => d.CreatedOn).Take(500).ToListAsync(ct));
    }
    [HttpPost("{id:guid}/review")] public async Task<IActionResult> Review(Guid id, ReviewDecl i, CancellationToken ct)
    {
        if (i.Status is not ("Approved" or "Rejected")) return BadRequest(new { message = "وضعیت نامعتبر" });
        var me = this.Uid(); if (me == null) return Unauthorized();
        var d = await _db.Set<PaymentDeclaration>().FindAsync(new object[] { id }, ct); if (d == null || d.TenantId != this.Tid()) return NotFound();
        if (d.Status != "Pending") return Conflict(new { message = "این مورد قبلاً بررسی شده است" });
        if (d.UserId == me) return StatusCode(403, new { message = "واریز خودتان را نمی‌توانید تأیید کنید" });
        if (i.Status == "Rejected" && string.IsNullOrWhiteSpace(i.Note)) return BadRequest(new { message = "برای رد کردن، دلیل را بنویسید" });
        d.Status = i.Status; d.ReviewNote = i.Note?.Trim(); d.ReviewedBy = me; d.ReviewedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct); return Ok(new { d.Status });
    }
}

// ===== طرف‌حساب‌ها =====
public record CounterInput(string Name, string Iban, bool IsActive);
[ApiController, Route("api/v1/admin/counterparties"), Authorize(Roles = "Admin,SuperAdmin")]
public class CounterPartiesController : ControllerBase
{
    private readonly AppDbContext _db; public CounterPartiesController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await _db.Set<CounterParty>().Where(c => c.TenantId == this.Tid()).OrderBy(c => c.Name).ToListAsync(ct));
    [HttpPost] public async Task<IActionResult> Create(CounterInput i, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(i.Name)) return BadRequest(new { message = "نام الزامی است" });
        var iban = (i.Iban ?? "").Replace(" ", "").ToUpperInvariant(); if (!OpsU.IbanOk(iban)) return BadRequest(new { message = "شماره شبا معتبر نیست" });
        if (await _db.Set<CounterParty>().AnyAsync(c => c.TenantId == this.Tid() && c.Iban == iban, ct)) return Conflict(new { message = "این شبا قبلاً ثبت شده" });
        var c = new CounterParty { TenantId = this.Tid(), Name = i.Name.Trim(), Iban = iban, IsActive = i.IsActive }; _db.Add(c); await _db.SaveChangesAsync(ct); return Ok(new { c.Id });
    }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, CounterInput i, CancellationToken ct)
    {
        var c = await _db.Set<CounterParty>().FindAsync(new object[] { id }, ct); if (c == null || c.TenantId != this.Tid()) return NotFound();
        var iban = (i.Iban ?? "").Replace(" ", "").ToUpperInvariant(); if (string.IsNullOrWhiteSpace(i.Name) || !OpsU.IbanOk(iban)) return BadRequest(new { message = "نام یا شبا نامعتبر است" });
        c.Name = i.Name.Trim(); c.Iban = iban; c.IsActive = i.IsActive; await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ===== بازاریاب‌ها =====
public record MarketerInput(string Name, string Phone, decimal CommissionPercent, bool IsActive);
[ApiController, Route("api/v1/admin/marketers"), Authorize(Roles = "Admin,SuperAdmin")]
public class MarketersController : ControllerBase
{
    private readonly AppDbContext _db; public MarketersController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await _db.Set<Marketer>().Where(m => m.TenantId == this.Tid()).OrderBy(m => m.Name).ToListAsync(ct));
    static string? Check(MarketerInput i, out string phone)
    {
        phone = OpsU.Digits(i.Phone);
        if (string.IsNullOrWhiteSpace(i.Name)) return "نام الزامی است";
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, "^09\\d{9}$")) return "شماره موبایل معتبر نیست";
        if (i.CommissionPercent < 0 || i.CommissionPercent > 100) return "درصد پورسانت بین ۰ تا ۱۰۰ باشد";
        return null;
    }
    [HttpPost] public async Task<IActionResult> Create(MarketerInput i, CancellationToken ct)
    {
        var e = Check(i, out var phone); if (e != null) return BadRequest(new { message = e });
        if (await _db.Set<Marketer>().AnyAsync(m => m.TenantId == this.Tid() && m.Phone == phone, ct)) return Conflict(new { message = "این شماره قبلاً ثبت شده" });
        string code; do { code = "MK" + System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999); } while (await _db.Set<Marketer>().AnyAsync(m => m.TenantId == this.Tid() && m.Code == code, ct));
        var m2 = new Marketer { TenantId = this.Tid(), Name = i.Name.Trim(), Phone = phone, Code = code, CommissionPercent = i.CommissionPercent, IsActive = i.IsActive };
        _db.Add(m2); await _db.SaveChangesAsync(ct); return Ok(new { m2.Id, m2.Code });
    }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, MarketerInput i, CancellationToken ct)
    {
        var m = await _db.Set<Marketer>().FindAsync(new object[] { id }, ct); if (m == null || m.TenantId != this.Tid()) return NotFound();
        var e = Check(i, out var phone); if (e != null) return BadRequest(new { message = e });
        m.Name = i.Name.Trim(); m.Phone = phone; m.CommissionPercent = i.CommissionPercent; m.IsActive = i.IsActive; await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ===== تنظیمات اپلیکیشن =====
[ApiController, Route("api/v1")]
public class AppSettingsController : ControllerBase
{
    static readonly string[] Keys = { "latestVersion", "minVersion", "apkUrl", "supportPhone", "forceUpdate", "maintenance" };
    private readonly AppDbContext _db; public AppSettingsController(AppDbContext db) => _db = db;
    async Task<Dictionary<string, string>> Load(CancellationToken ct)
    { var tid = this.Tid(); var l = await _db.Set<AppSetting>().Where(s => s.TenantId == tid).ToListAsync(ct); return Keys.ToDictionary(k => k, k => l.FirstOrDefault(s => s.Key == k)?.Value ?? ""); }
    /// برای اپ موبایل (بدون ورود).
    [HttpGet("app/config"), AllowAnonymous, ResponseCache(Duration = 30)]
    public async Task<IActionResult> Public(CancellationToken ct) => Ok(await Load(ct));
    [HttpGet("admin/app-settings"), Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await Load(ct));
    [HttpPut("admin/app-settings"), Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Put([FromBody] Dictionary<string, string> body, CancellationToken ct)
    {
        var tid = this.Tid();
        if (body.TryGetValue("apkUrl", out var u) && !string.IsNullOrWhiteSpace(u) && !u.StartsWith("https://")) return BadRequest(new { message = "لینک دانلود باید با https شروع شود" });
        foreach (var kv in body.Where(k => Keys.Contains(k.Key)))
        {
            var v = (kv.Value ?? "").Trim(); if (v.Length > 500) return BadRequest(new { message = "مقدار خیلی بلند است" });
            var s = await _db.Set<AppSetting>().FirstOrDefaultAsync(x => x.TenantId == tid && x.Key == kv.Key, ct);
            if (s == null) _db.Add(new AppSetting { TenantId = tid, Key = kv.Key, Value = v }); else s.Value = v;
        }
        await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ===== استعلام‌ها =====
public record InquiryCreate(Guid UserId, string Type, string? Note);
public record InquiryResult(string Status, string? Result);
[ApiController, Route("api/v1/admin/inquiries"), Authorize(Roles = "Admin,SuperAdmin")]
public class InquiriesController : ControllerBase
{
    private readonly AppDbContext _db; public InquiriesController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    { var q = _db.Set<InquiryRequest>().Where(x => x.TenantId == this.Tid()); if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status); return Ok(await q.OrderByDescending(x => x.CreatedOn).Take(500).ToListAsync(ct)); }
    [HttpPost] public async Task<IActionResult> Create(InquiryCreate i, CancellationToken ct)
    {
        if (i.Type is not ("Credit" or "Identity" or "Other")) return BadRequest(new { message = "نوع استعلام نامعتبر است" });
        if (i.UserId == Guid.Empty) return BadRequest(new { message = "شناسه‌ی مشتری الزامی است" });
        var x = new InquiryRequest { TenantId = this.Tid(), UserId = i.UserId, Type = i.Type, Note = (i.Note ?? "").Trim() }; if (x.Note.Length > 300) x.Note = x.Note[..300];
        _db.Add(x); await _db.SaveChangesAsync(ct); return Ok(new { x.Id });
    }
    [HttpPost("{id:guid}/result")] public async Task<IActionResult> Result(Guid id, InquiryResult i, CancellationToken ct)
    {
        if (i.Status is not ("Done" or "Failed")) return BadRequest(new { message = "وضعیت نامعتبر" });
        var x = await _db.Set<InquiryRequest>().FindAsync(new object[] { id }, ct); if (x == null || x.TenantId != this.Tid()) return NotFound();
        if (x.Status != "Queued") return Conflict(new { message = "نتیجه‌ی این استعلام قبلاً ثبت شده" });
        x.Status = i.Status; x.Result = (i.Result ?? "").Trim(); if (x.Result.Length > 300) x.Result = x.Result[..300]; x.DoneAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ===== سفارش خریدها =====
public record OrderCreate(Guid UserId, Guid? LoanId, string MerchantName, decimal Amount);
public record OrderStatus(string Status);
[ApiController, Route("api/v1/admin/orders"), Authorize(Roles = "Admin,SuperAdmin")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db; public OrdersController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok(await _db.Set<PurchaseOrder>().Where(o => o.TenantId == this.Tid()).OrderByDescending(o => o.CreatedOn).Take(500).ToListAsync(ct));
    [HttpPost] public async Task<IActionResult> Create(OrderCreate i, CancellationToken ct)
    {
        if (i.UserId == Guid.Empty || string.IsNullOrWhiteSpace(i.MerchantName) || i.Amount <= 0) return BadRequest(new { message = "مشتری، پذیرنده و مبلغ الزامی است" });
        string code; do { code = "O" + System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999); } while (await _db.Set<PurchaseOrder>().AnyAsync(o => o.TenantId == this.Tid() && o.Code == code, ct));
        var o2 = new PurchaseOrder { TenantId = this.Tid(), UserId = i.UserId, LoanId = i.LoanId, MerchantName = i.MerchantName.Trim(), Amount = i.Amount, Code = code };
        _db.Add(o2); await _db.SaveChangesAsync(ct); return Ok(new { o2.Id, o2.Code });
    }
    [HttpPost("{id:guid}/status")] public async Task<IActionResult> SetStatus(Guid id, OrderStatus i, CancellationToken ct)
    {
        if (i.Status is not ("Delivered" or "Cancelled")) return BadRequest(new { message = "وضعیت نامعتبر" });
        var o = await _db.Set<PurchaseOrder>().FindAsync(new object[] { id }, ct); if (o == null || o.TenantId != this.Tid()) return NotFound();
        if (o.Status != "Pending") return Conflict(new { message = "وضعیت این سفارش قبلاً نهایی شده" });
        o.Status = i.Status; await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ===== پشتیبان‌گیری پایگاه داده (فقط سوپرادمین) =====
// بازگردانی عمداً از طریق API ارائه نمی‌شود: جایگزینی دیتابیس زنده باید دستی و با توقف سرویس انجام شود.
[ApiController, Route("api/v1/admin/backups"), Authorize(Roles = "SuperAdmin")]
public class BackupsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IConfiguration _cfg; private readonly ILogger<BackupsController> _log;
    public BackupsController(AppDbContext db, IConfiguration cfg, ILogger<BackupsController> log) { _db = db; _cfg = cfg; _log = log; }
    string? Dir => _cfg["Backup:Directory"];
    static readonly System.Text.RegularExpressions.Regex NameRx = new("^[A-Za-z0-9._-]{3,120}\\.bak$", System.Text.RegularExpressions.RegexOptions.Compiled);
    [HttpGet] public IActionResult List()
    {
        if (string.IsNullOrWhiteSpace(Dir)) return StatusCode(503, new { message = "مسیر Backup:Directory در تنظیمات سرور مشخص نشده است" });
        if (!Directory.Exists(Dir)) return Ok(Array.Empty<object>());
        return Ok(new DirectoryInfo(Dir).GetFiles("*.bak").OrderByDescending(f => f.LastWriteTimeUtc).Take(50).Select(f => new { name = f.Name, size = f.Length, createdOn = f.LastWriteTimeUtc }));
    }
    [HttpPost] public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Dir)) return StatusCode(503, new { message = "مسیر Backup:Directory در تنظیمات سرور مشخص نشده است" });
        var dbName = _db.Database.GetDbConnection().Database; if (string.IsNullOrWhiteSpace(dbName)) return StatusCode(500, new { message = "نام پایگاه داده پیدا نشد" });
        var safe = new string(dbName.Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-').ToArray());
        var path = Path.Combine(Dir, $"{safe}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");
        try
        {
            _db.Database.SetCommandTimeout(900);
            // فایل را خود SQL Server می‌نویسد؛ پوشه باید برای سرویس SQL Server قابل نوشتن باشد.
            await _db.Database.ExecuteSqlRawAsync("BACKUP DATABASE [" + dbName.Replace("]", "]]") + "] TO DISK = @p0 WITH INIT, CHECKSUM", new object[] { path }, ct);
            _log.LogInformation("Database backup created {File}", Path.GetFileName(path));
            return Ok(new { name = Path.GetFileName(path) });
        }
        catch (Exception e) { _log.LogError(e, "Backup failed"); return StatusCode(500, new { message = "تهیه‌ی پشتیبان ناموفق بود. دسترسی SQL Server به پوشه را بررسی کنید." }); }
    }
    [HttpGet("{name}")] public IActionResult Download(string name)
    {
        if (string.IsNullOrWhiteSpace(Dir) || !NameRx.IsMatch(name)) return NotFound();
        var p = Path.Combine(Dir, name); if (!System.IO.File.Exists(p)) return NotFound();
        return PhysicalFile(p, "application/octet-stream", name);
    }
}
