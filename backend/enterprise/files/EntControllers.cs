using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ghestine.Domain.Entities;
using Ghestine.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using __DBNS__;
namespace Ghestine.API.Enterprise;

/// همه‌ی درخواست‌های تغییردهنده‌ی مدیریتی (POST/PUT/DELETE روی api/v1/admin) را خودکار در زنجیره‌ی حسابرسی می‌نویسد. بدنه‌ی درخواست ذخیره نمی‌شود (اطلاعات حساس).
public class AuditFilter : IAsyncActionFilter
{
    static readonly SemaphoreSlim Gate = new(1, 1);
    private readonly AppDbContext _db; public AuditFilter(AppDbContext db) => _db = db;
    public static string Compute(string prev, AuditEntry e) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", prev, e.Seq, e.TenantId, e.UserId, e.Method, e.Path, e.StatusCode, e.At.ToString("O"))))).ToLowerInvariant();
    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var r = ctx.HttpContext.Request; var done = await next();
        if (r.Method == "GET" || r.Method == "HEAD" || r.Method == "OPTIONS" || !r.Path.StartsWithSegments("/api/v1/admin")) return;
        try
        {
            var u = ctx.HttpContext.User; Guid? uid = Guid.TryParse(u.FindFirstValue(ClaimTypes.NameIdentifier) ?? u.FindFirstValue("sub"), out var g) ? g : null;
            var tid = (ctx.HttpContext.Items["tenant"] as TenantInfo)?.Id ?? Guid.Empty;
            await Gate.WaitAsync();
            try
            {
                var last = await _db.Set<AuditEntry>().Where(a => a.TenantId == tid).OrderByDescending(a => a.Seq).FirstOrDefaultAsync();
                var e = new AuditEntry { TenantId = tid, Seq = (last?.Seq ?? 0) + 1, UserId = uid, UserName = (u.Identity?.Name ?? "").Truncate(120), Method = r.Method, Path = r.Path.Value!.Truncate(300), StatusCode = ctx.HttpContext.Response.StatusCode, Ip = (ctx.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "").Truncate(45), At = DateTime.UtcNow, PrevHash = last?.Hash ?? new string('0', 64) };
                if (done.Exception != null && !done.ExceptionHandled) e.StatusCode = 500;
                e.Hash = Compute(e.PrevHash, e); _db.Add(e); await _db.SaveChangesAsync();
            }
            finally { Gate.Release(); }
        }
        catch { /* حسابرسی نباید عملیات اصلی را خراب کند؛ خطا در لاگ برنامه دیده می‌شود */ }
    }
}
static class EntU { public static string Truncate(this string s, int n) => s.Length <= n ? s : s[..n]; }

[ApiController, Route("api/v1/admin/audit"), Authorize(Roles = "Admin,SuperAdmin")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _db; public AuditController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List([FromQuery] int take = 200, CancellationToken ct = default)
        => Ok(await _db.Set<AuditEntry>().Where(a => a.TenantId == this.Tid()).OrderByDescending(a => a.Seq).Take(Math.Clamp(take, 1, 500)).Select(a => new { a.Seq, a.UserName, a.Method, a.Path, a.StatusCode, a.Ip, a.At }).ToListAsync(ct));
    [HttpGet("verify"), Authorize(Roles = "SuperAdmin")] public async Task<IActionResult> Verify(CancellationToken ct)
    {
        string prev = new string('0', 64); long n = 0, expect = 1;
        await foreach (var a in _db.Set<AuditEntry>().Where(x => x.TenantId == this.Tid()).OrderBy(x => x.Seq).AsAsyncEnumerable().WithCancellation(ct))
        {
            if (a.Seq != expect || a.PrevHash != prev || a.Hash != AuditFilter.Compute(prev, a)) return Ok(new { ok = false, brokenAt = a.Seq, checkedCount = n });
            prev = a.Hash; expect++; n++;
        }
        return Ok(new { ok = true, checkedCount = n });
    }
}

public record AccountIn(string Code, string Name, string Type);
public record LineIn(Guid AccountId, decimal Debit, decimal Credit);
public record EntryIn(string Memo, string? Reference, List<LineIn> Lines);
[ApiController, Route("api/v1/admin/ledger"), Authorize(Roles = "Admin,SuperAdmin")]
public class LedgerController : ControllerBase
{
    private readonly AppDbContext _db; public LedgerController(AppDbContext db) => _db = db;
    static readonly string[] Types = { "Asset", "Liability", "Equity", "Income", "Expense" };
    [HttpGet("accounts")] public async Task<IActionResult> Accounts(CancellationToken ct) => Ok(await _db.Set<LedgerAccount>().Where(a => a.TenantId == this.Tid()).OrderBy(a => a.Code).ToListAsync(ct));
    [HttpPost("accounts")] public async Task<IActionResult> AddAccount(AccountIn i, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(i.Code) || i.Code.Length > 20 || string.IsNullOrWhiteSpace(i.Name) || !Types.Contains(i.Type)) return BadRequest(new { message = "کد، نام و نوع حساب را درست وارد کنید" });
        if (await _db.Set<LedgerAccount>().AnyAsync(a => a.TenantId == this.Tid() && a.Code == i.Code.Trim(), ct)) return Conflict(new { message = "این کد قبلاً وجود دارد" });
        _db.Add(new LedgerAccount { TenantId = this.Tid(), Code = i.Code.Trim(), Name = i.Name.Trim(), Type = i.Type }); await _db.SaveChangesAsync(ct); return Ok();
    }
    [HttpGet("entries")] public async Task<IActionResult> Entries(CancellationToken ct)
    {
        var es = await _db.Set<JournalEntry>().Where(e => e.TenantId == this.Tid()).OrderByDescending(e => e.Number).Take(100).ToListAsync(ct); var ids = es.Select(e => e.Id).ToList();
        var ls = await _db.Set<JournalLine>().Where(l => ids.Contains(l.EntryId)).ToListAsync(ct);
        return Ok(es.Select(e => new { e.Id, e.Number, e.Memo, e.Reference, e.ReversalOf, e.CreatedOn, debit = ls.Where(l => l.EntryId == e.Id).Sum(l => l.Debit) }));
    }
    [HttpPost("entries")] public async Task<IActionResult> Post(EntryIn i, CancellationToken ct)
    {
        if (i.Lines == null || i.Lines.Count < 2 || i.Lines.Count > 50) return BadRequest(new { message = "سند باید حداقل دو سطر داشته باشد" });
        if (i.Lines.Any(l => l.Debit < 0 || l.Credit < 0 || (l.Debit > 0) == (l.Credit > 0))) return BadRequest(new { message = "هر سطر فقط یکی از بدهکار یا بستانکار را با مقدار مثبت دارد" });
        if (i.Lines.Sum(l => l.Debit) != i.Lines.Sum(l => l.Credit)) return BadRequest(new { message = "جمع بدهکار و بستانکار برابر نیست" });
        var accIds = i.Lines.Select(l => l.AccountId).Distinct().ToList();
        if (await _db.Set<LedgerAccount>().CountAsync(a => a.TenantId == this.Tid() && a.IsActive && accIds.Contains(a.Id), ct) != accIds.Count) return BadRequest(new { message = "حساب نامعتبر یا غیرفعال" });
        return await Save(i.Memo, i.Reference, i.Lines, null, ct);
    }
    [HttpPost("entries/{id:guid}/reverse")] public async Task<IActionResult> Reverse(Guid id, CancellationToken ct)
    {
        var e = await _db.Set<JournalEntry>().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == this.Tid(), ct); if (e == null) return NotFound();
        if (e.ReversalOf != null || await _db.Set<JournalEntry>().AnyAsync(x => x.ReversalOf == id, ct)) return Conflict(new { message = "این سند معکوس است یا قبلاً معکوس شده" });
        var ls = await _db.Set<JournalLine>().Where(l => l.EntryId == id).ToListAsync(ct);
        return await Save("معکوس سند " + e.Number, e.Reference, ls.Select(l => new LineIn(l.AccountId, l.Credit, l.Debit)).ToList(), id, ct);
    }
    async Task<IActionResult> Save(string memo, string? reference, List<LineIn> lines, Guid? rev, CancellationToken ct)
    {
        // شماره‌ی سند پشت سر هم است؛ کلید یکتا روی (TenantId, Number) در صورت رقابت، یکی را رد می‌کند.
        for (var tries = 0; tries < 3; tries++)
        {
            var max = await _db.Set<JournalEntry>().Where(x => x.TenantId == this.Tid()).MaxAsync(x => (long?)x.Number, ct) ?? 0;
            var e = new JournalEntry { TenantId = this.Tid(), Number = max + 1, Memo = (memo ?? "").Trim().Truncate(300), Reference = (reference ?? "").Trim().Truncate(60), ReversalOf = rev, CreatedBy = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var g) ? g : null };
            _db.Add(e); foreach (var l in lines) _db.Add(new JournalLine { TenantId = this.Tid(), EntryId = e.Id, AccountId = l.AccountId, Debit = l.Debit, Credit = l.Credit });
            try { await _db.SaveChangesAsync(ct); return Ok(new { e.Id, e.Number }); }
            catch (DbUpdateException) { _db.ChangeTracker.Clear(); }
        }
        return Conflict(new { message = "ثبت سند به‌دلیل رقابت ناموفق بود؛ دوباره تلاش کنید" });
    }
    [HttpGet("trial-balance")] public async Task<IActionResult> Trial(CancellationToken ct)
    {
        var accs = await _db.Set<LedgerAccount>().Where(a => a.TenantId == this.Tid()).OrderBy(a => a.Code).ToListAsync(ct);
        var sums = await _db.Set<JournalLine>().Where(l => l.TenantId == this.Tid()).GroupBy(l => l.AccountId).Select(g => new { g.Key, d = g.Sum(x => x.Debit), c = g.Sum(x => x.Credit) }).ToListAsync(ct);
        var rows = accs.Select(a => { var s = sums.FirstOrDefault(x => x.Key == a.Id); return new { a.Code, a.Name, a.Type, debit = s?.d ?? 0m, credit = s?.c ?? 0m }; }).ToList();
        return Ok(new { rows, totalDebit = rows.Sum(r => r.debit), totalCredit = rows.Sum(r => r.credit), balanced = rows.Sum(r => r.debit) == rows.Sum(r => r.credit) });
    }
}

/// سلامت برنامه برای مانیتورینگ بیرونی (UptimeRobot و مشابه). بدون اطلاعات حساس.
[ApiController, Route("api/v1/health"), AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db; public HealthController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct)
    {
        try { var ok = await _db.Database.CanConnectAsync(ct); return ok ? Ok(new { status = "ok" }) : StatusCode(503, new { status = "db-down" }); }
        catch { return StatusCode(503, new { status = "db-down" }); }
    }
}

/// پشتیبان‌گیری خودکار روزانه (ساعت Backup:HourUtc، پیش‌فرض ۲۳:۰۰ UTC). فقط وقتی Backup:Directory تنظیم باشد کار می‌کند؛ ۱۴ فایل آخر نگه داشته می‌شود.
public class DailyBackupService : BackgroundService
{
    private readonly IServiceScopeFactory _sf; private readonly IConfiguration _cfg; private readonly ILogger<DailyBackupService> _log;
    public DailyBackupService(IServiceScopeFactory sf, IConfiguration cfg, ILogger<DailyBackupService> log) { _sf = sf; _cfg = cfg; _log = log; }
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var dir = _cfg["Backup:Directory"]; if (string.IsNullOrWhiteSpace(dir)) { _log.LogInformation("Daily backup disabled (Backup:Directory not set)"); return; }
        var hour = int.TryParse(_cfg["Backup:HourUtc"], out var h) && h is >= 0 and <= 23 ? h : 23;
        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.UtcNow; var next = now.Date.AddHours(hour); if (next <= now) next = next.AddDays(1);
            try { await Task.Delay(next - now, ct); } catch (OperationCanceledException) { return; }
            try
            {
                using var sc = _sf.CreateScope(); var db = sc.ServiceProvider.GetRequiredService<AppDbContext>();
                var name = new string(db.Database.GetDbConnection().Database.Where(c => char.IsLetterOrDigit(c) || c is '_' or '-').ToArray());
                var path = Path.Combine(dir, $"{name}-auto-{DateTime.UtcNow:yyyyMMdd-HHmmss}.bak");
                db.Database.SetCommandTimeout(1800);
                await db.Database.ExecuteSqlRawAsync("BACKUP DATABASE [" + db.Database.GetDbConnection().Database.Replace("]", "]]") + "] TO DISK = @p0 WITH INIT, CHECKSUM", new object[] { path }, ct);
                _log.LogInformation("Auto backup created {File}", Path.GetFileName(path));
                if (Directory.Exists(dir)) foreach (var f in new DirectoryInfo(dir).GetFiles(name + "-auto-*.bak").OrderByDescending(f => f.LastWriteTimeUtc).Skip(14)) { try { f.Delete(); } catch { } }
            }
            catch (Exception e) when (e is not OperationCanceledException) { _log.LogError(e, "Auto backup failed"); }
        }
    }
}
