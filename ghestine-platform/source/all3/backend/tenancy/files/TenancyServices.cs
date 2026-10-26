using System.Text.Json;
using System.Text.RegularExpressions;
using Ghestine.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using __DBNS__;
namespace Ghestine.API.Tenancy;

public record TenantInfo(Guid Id, string Name, string Slug, string BrandJson);

/// از روی نام دامنه‌ی درخواست، مستأجر را پیدا می‌کند. دامنه‌ی ناشناس به مستأجر پیش‌فرض می‌رسد.
public class TenantMiddleware
{
    private readonly RequestDelegate _next; private readonly IMemoryCache _c;
    public TenantMiddleware(RequestDelegate next, IMemoryCache c) { _next = next; _c = c; }
    public static string HostOf(HttpContext ctx)
    {
        var h = ctx.Request.Host.Host;
        // X-Forwarded-Host فقط وقتی معتبر است که از پراکسی محلی (nginx روی همین سرور) آمده باشد
        var ip = ctx.Connection.RemoteIpAddress;
        if (ip != null && System.Net.IPAddress.IsLoopback(ip) && ctx.Request.Headers.TryGetValue("X-Forwarded-Host", out var xf) && !string.IsNullOrWhiteSpace(xf))
            h = xf.ToString().Split(',')[0].Trim().Split(':')[0];
        return h.ToLowerInvariant();
    }
    public async Task Invoke(HttpContext ctx, AppDbContext db)
    {
        var host = HostOf(ctx);
        TenantInfo? t;
        try
        {
            t = await _c.GetOrCreateAsync("tenant:" + host, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                var td = await db.Set<TenantDomain>().AsNoTracking().FirstOrDefaultAsync(x => x.Host == host);
                Tenant? tn = td == null ? null : await db.Set<Tenant>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == td.TenantId && x.IsActive);
                tn ??= await db.Set<Tenant>().AsNoTracking().FirstOrDefaultAsync(x => x.IsDefault && x.IsActive);
                if (tn == null) // اولین اجرا: مستأجر پیش‌فرض ساخته می‌شود
                {
                    tn = new Tenant { Id = Guid.NewGuid(), Name = "دیجی‌بایل پی", Slug = "default", IsDefault = true, BrandJson = "{\"logoText\":\"د\"}" };
                    db.Add(tn); await db.SaveChangesAsync();
                }
                return new TenantInfo(tn.Id, tn.Name, tn.Slug, tn.BrandJson);
            });
        }
        catch (Exception ex)
        {
            // مثلاً مایگریشن هنوز زده نشده: سایت قدیمی از کار نمی‌افتد و فقط قابلیت چندمستاجری غیرفعال می‌ماند.
            db.ChangeTracker.Clear();
            ctx.RequestServices.GetService<ILogger<TenantMiddleware>>()?.LogError(ex, "Tenant lookup failed");
            t = new TenantInfo(Guid.Empty, "دیجی‌بایل پی", "default", "{}");
        }
        ctx.Items["tenant"] = t;
        await _next(ctx);
    }
}

public static class TenantExt
{
    public static TenantInfo? Tenant(this ControllerBase c) => c.HttpContext.Items["tenant"] as TenantInfo;
    public static Guid Tid(this ControllerBase c) => c.Tenant()?.Id ?? Guid.Empty;
    public static string Brand(this ControllerBase c) => c.Tenant()?.Name ?? "دیجی‌بایل پی";
}

// ---- عمومی: برند و تنظیمات ظاهری دامنه‌ی فعلی (صفحه‌ها این را می‌خوانند) ----
[ApiController, Route("api/v1/tenant"), AllowAnonymous]
public class TenantPublicController : ControllerBase
{
    [HttpGet("config"), ResponseCache(Duration = 60)]
    public IActionResult Config()
    {
        var t = this.Tenant(); if (t == null) return NotFound();
        var brand = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(t.BrandJson) ? "{}" : t.BrandJson);
        return Ok(new { name = t.Name, slug = t.Slug, brand });
    }
}

// ---- مدیریت مستأجرها و دامنه‌ها (فقط SuperAdmin) ----
public record TenantInput(string Name, string Slug, JsonElement Brand, bool IsActive);
public record DomainInput(string Host, bool IsPrimary);
[ApiController, Route("api/v1/admin/tenants"), Authorize(Roles = "SuperAdmin")]
public class TenantAdminController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IMemoryCache _c;
    static readonly Regex HostRx = new("^(?=.{4,253}$)([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\\.)+[a-z]{2,}$", RegexOptions.Compiled);
    static readonly Regex SlugRx = new("^[a-z0-9-]{2,40}$", RegexOptions.Compiled);
    public TenantAdminController(AppDbContext db, IMemoryCache c) { _db = db; _c = c; }
    void Flush(IEnumerable<string> hosts) { foreach (var h in hosts) _c.Remove("tenant:" + h); }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var ts = await _db.Set<Tenant>().AsNoTracking().OrderBy(t => t.CreatedOn).ToListAsync(ct);
        var ds = await _db.Set<TenantDomain>().AsNoTracking().ToListAsync(ct);
        return Ok(ts.Select(t => new { t.Id, t.Name, t.Slug, t.IsDefault, t.IsActive, brand = JsonSerializer.Deserialize<JsonElement>(t.BrandJson), domains = ds.Where(d => d.TenantId == t.Id).Select(d => new { d.Id, d.Host, d.IsPrimary }) }));
    }
    [HttpPost]
    public async Task<IActionResult> Create(TenantInput i, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(i.Name) || !SlugRx.IsMatch(i.Slug ?? "")) return BadRequest(new { message = "نام و شناسه‌ی انگلیسی معتبر لازم است" });
        if (await _db.Set<Tenant>().AnyAsync(t => t.Slug == i.Slug, ct)) return Conflict(new { message = "این شناسه قبلاً ثبت شده" });
        var t = new Tenant { Id = Guid.NewGuid(), Name = i.Name.Trim(), Slug = i.Slug!, IsActive = i.IsActive, BrandJson = i.Brand.ValueKind == JsonValueKind.Object ? i.Brand.GetRawText() : "{}" };
        _db.Add(t); await _db.SaveChangesAsync(ct); return Ok(new { t.Id });
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TenantInput i, CancellationToken ct)
    {
        var t = await _db.Set<Tenant>().FindAsync(new object[] { id }, ct); if (t == null) return NotFound();
        if (string.IsNullOrWhiteSpace(i.Name)) return BadRequest(new { message = "نام الزامی است" });
        if (t.IsDefault && !i.IsActive) return BadRequest(new { message = "مستأجر پیش‌فرض غیرفعال نمی‌شود" });
        t.Name = i.Name.Trim(); t.IsActive = i.IsActive; if (i.Brand.ValueKind == JsonValueKind.Object) t.BrandJson = i.Brand.GetRawText();
        await _db.SaveChangesAsync(ct);
        Flush(await _db.Set<TenantDomain>().Where(d => d.TenantId == id).Select(d => d.Host).ToListAsync(ct)); return Ok();
    }
    [HttpPost("{id:guid}/domains")]
    public async Task<IActionResult> AddDomain(Guid id, DomainInput i, CancellationToken ct)
    {
        var host = (i.Host ?? "").Trim().ToLowerInvariant();
        if (!HostRx.IsMatch(host)) return BadRequest(new { message = "نام دامنه معتبر نیست (مثال: shop.example.ir)" });
        if (!await _db.Set<Tenant>().AnyAsync(t => t.Id == id, ct)) return NotFound();
        if (await _db.Set<TenantDomain>().AnyAsync(d => d.Host == host, ct)) return Conflict(new { message = "این دامنه قبلاً به یک مستأجر وصل شده" });
        if (i.IsPrimary) foreach (var d in await _db.Set<TenantDomain>().Where(d => d.TenantId == id && d.IsPrimary).ToListAsync(ct)) d.IsPrimary = false;
        _db.Add(new TenantDomain { Id = Guid.NewGuid(), TenantId = id, Host = host, IsPrimary = i.IsPrimary }); await _db.SaveChangesAsync(ct); Flush(new[] { host });
        return Ok(new { host, nextSteps = "رکورد DNS دامنه را به IP سرور بزنید، دامنه را در DirectAdmin/nginx اضافه کنید و گواهی SSL بگیرید." });
    }
    [HttpDelete("{id:guid}/domains/{domainId:guid}")]
    public async Task<IActionResult> DelDomain(Guid id, Guid domainId, CancellationToken ct)
    {
        var d = await _db.Set<TenantDomain>().FirstOrDefaultAsync(x => x.Id == domainId && x.TenantId == id, ct); if (d == null) return NotFound();
        _db.Remove(d); await _db.SaveChangesAsync(ct); Flush(new[] { d.Host }); return NoContent();
    }
}
