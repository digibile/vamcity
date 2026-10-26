using System.Security.Claims;
using System.Text.Json;
using Ghestine.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ghestine.API.Tenancy;
using __DBNS__;
namespace Ghestine.API.Core;

static class U
{
    public static Guid? Uid(this ControllerBase c) => Guid.TryParse(c.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? c.User.FindFirstValue("sub"), out var g) ? g : null;
    public static string ClientIp(this ControllerBase c)
    {
        var ra = c.HttpContext.Connection.RemoteIpAddress;
        if (ra != null && System.Net.IPAddress.IsLoopback(ra))
        {
            var xf = c.Request.Headers["X-Real-IP"].FirstOrDefault() ?? c.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();
            if (!string.IsNullOrWhiteSpace(xf) && System.Net.IPAddress.TryParse(xf, out var ip)) return ip.ToString();
        }
        return ra?.ToString() ?? "unknown";
    }
    public static string Mask(VirtualCard c) => $"{c.Bin} •••• •••• {c.Last4}";
    public static object Dto(VirtualCard c) => new { c.Id, c.LoanId, c.HolderName, number = Mask(c), expiry = $"{c.ExpYear}/{c.ExpMonth:00}", c.Limit, c.Spent, remaining = c.Limit - c.Spent, c.PerTxnLimit, c.Status, c.CreatedOn };
}

// ========== محصولات وام: عمومی ==========
[ApiController, Route("api/v1/loan-products")]
public class LoanProductsPublicController : ControllerBase
{
    private readonly AppDbContext _db; public LoanProductsPublicController(AppDbContext db) => _db = db;
    [HttpGet, AllowAnonymous, ResponseCache(Duration = 60)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var tid = this.Tid(); var l = await _db.Set<LoanProduct>().Where(p => p.TenantId == tid && p.IsActive).OrderBy(p => p.SortOrder).ToListAsync(ct);
        return Ok(l.Select(Shape));
    }
    [HttpGet("{slug}"), AllowAnonymous]
    public async Task<IActionResult> One(string slug, CancellationToken ct)
    {
        var p = await _db.Set<LoanProduct>().FirstOrDefaultAsync(x => x.TenantId == this.Tid() && x.IsActive && (x.Slug == slug), ct);
        return p == null ? NotFound() : Ok(Shape(p));
    }
    internal static object Shape(LoanProduct p) => new
    {
        id = p.Slug, name = p.Name, description = p.Description, badge = p.Badge, isActive = p.IsActive,
        minAmount = p.MinAmount, maxAmount = p.MaxAmount, intervalDays = p.IntervalDays,
        tiers = JsonSerializer.Deserialize<JsonElement>(p.TiersJson),
        conditions = JsonSerializer.Deserialize<JsonElement>(p.ConditionsJson),
        documents = JsonSerializer.Deserialize<JsonElement>(p.DocumentsJson)
    };
}

// ========== محصولات وام: مدیر ==========
public record ProductInput(string Name, string Slug, string? Description, string? Badge, decimal MinAmount, decimal MaxAmount, int IntervalDays,
    JsonElement Tiers, JsonElement Conditions, JsonElement Documents, bool IsActive, int SortOrder);

[ApiController, Route("api/v1/admin/loan-products"), Authorize(Roles = "Admin,SuperAdmin")]
public class LoanProductsAdminController : ControllerBase
{
    private readonly AppDbContext _db; public LoanProductsAdminController(AppDbContext db) => _db = db;
    [HttpGet] public async Task<IActionResult> List(CancellationToken ct) => Ok((await _db.Set<LoanProduct>().Where(p => p.TenantId == this.Tid()).OrderBy(p => p.SortOrder).ToListAsync(ct)).Select(p => new { p.Id, shape = LoanProductsPublicController.Shape(p), p.IsActive, p.SortOrder }));

    static string? Check(ProductInput i)
    {
        if (string.IsNullOrWhiteSpace(i.Name) || string.IsNullOrWhiteSpace(i.Slug)) return "نام و شناسه‌ی طرح الزامی است";
        if (!System.Text.RegularExpressions.Regex.IsMatch(i.Slug, "^[a-z0-9-]{2,40}$")) return "شناسه فقط حروف انگلیسی کوچک، عدد و خط تیره";
        var beta = i.Conditions.ValueKind == JsonValueKind.Object && i.Conditions.TryGetProperty("kind", out var kd) && kd.GetString() == "beta";
        if (beta) return null; // طرح خارجی: مبلغ و پله‌ها از API بانک می‌آید
        if (i.MinAmount <= 0 || i.MaxAmount < i.MinAmount) return "بازه‌ی مبلغ نامعتبر است";
        if (i.IntervalDays is < 1 or > 365) return "فاصله‌ی اقساط نامعتبر است";
        if (i.Tiers.ValueKind != JsonValueKind.Array || i.Tiers.GetArrayLength() == 0) return "حداقل یک پله لازم است";
        foreach (var t in i.Tiers.EnumerateArray())
            if (!t.TryGetProperty("months", out var m) || m.GetInt32() < 1 || !t.TryGetProperty("feePercent", out var f) || f.GetDecimal() < 0) return "پله‌ها نامعتبر است";
        return null;
    }
    static string? SafeCheck(ProductInput i) { try { return Check(i); } catch { return "پله‌ها یا شرایط نامعتبر است"; } }
    static void Fill(LoanProduct p, ProductInput i)
    {
        p.Name = i.Name.Trim(); p.Slug = i.Slug; p.Description = i.Description ?? ""; p.Badge = i.Badge ?? "";
        p.MinAmount = i.MinAmount; p.MaxAmount = i.MaxAmount; p.IntervalDays = i.IntervalDays;
        p.TiersJson = i.Tiers.GetRawText(); p.ConditionsJson = i.Conditions.ValueKind == JsonValueKind.Object ? i.Conditions.GetRawText() : "{}";
        p.DocumentsJson = i.Documents.ValueKind == JsonValueKind.Array ? i.Documents.GetRawText() : "[]";
        p.IsActive = i.IsActive; p.SortOrder = i.SortOrder;
    }
    [HttpPost] public async Task<IActionResult> Create(ProductInput i, CancellationToken ct)
    {
        var e = SafeCheck(i); if (e != null) return BadRequest(new { message = e });
        if (await _db.Set<LoanProduct>().AnyAsync(x => x.TenantId == this.Tid() && x.Slug == i.Slug, ct)) return Conflict(new { message = "این شناسه قبلاً ثبت شده" });
        var p = new LoanProduct { TenantId = this.Tid() }; Fill(p, i); _db.Add(p); await _db.SaveChangesAsync(ct); return Ok(new { p.Id });
    }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, ProductInput i, CancellationToken ct)
    {
        var e = SafeCheck(i); if (e != null) return BadRequest(new { message = e });
        var p = await _db.Set<LoanProduct>().FindAsync(new object[] { id }, ct); if (p == null || p.TenantId != this.Tid()) return NotFound();
        if (await _db.Set<LoanProduct>().AnyAsync(x => x.TenantId == this.Tid() && x.Slug == i.Slug && x.Id != id, ct)) return Conflict(new { message = "این شناسه قبلاً ثبت شده" });
        Fill(p, i); await _db.SaveChangesAsync(ct); return Ok();
    }
    [HttpPost("{id:guid}/toggle")] public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var p = await _db.Set<LoanProduct>().FindAsync(new object[] { id }, ct); if (p == null || p.TenantId != this.Tid()) return NotFound();
        p.IsActive = !p.IsActive; await _db.SaveChangesAsync(ct); return Ok(new { p.IsActive });
    }
    [HttpPost("seed")] public async Task<IActionResult> Seed(CancellationToken ct)
    {
        if (await _db.Set<LoanProduct>().AnyAsync(x => x.TenantId == this.Tid(), ct)) return Conflict(new { message = "محصولی از قبل وجود دارد" });
        string T(params (int m, double f, int d)[] x) => JsonSerializer.Serialize(x.Select(t => new { months = t.m, feePercent = t.f, downPaymentPercent = t.d }));
        const string C = "{\"minAge\":22,\"maxAge\":60,\"minIncome\":6,\"kyc\":1,\"minScore\":50,\"maxActive\":3}";
        const string D = "[{\"name\":\"کارت ملی\",\"required\":true},{\"name\":\"شناسنامه\",\"required\":true},{\"name\":\"فیش حقوقی\",\"required\":false}]";
        _db.AddRange(
            new LoanProduct { Name = "طرح بتا بانک رفاه", Slug = "beta", Description = "تسهیلات بانک رفاه؛ مبلغ و اقساط از سامانه بانک استعلام می‌شود", Badge = "جدید", MinAmount = 0, MaxAmount = 0, TiersJson = "[]", ConditionsJson = "{\"kind\":\"beta\"}", DocumentsJson = "[{\"name\":\"کارت ملی\",\"required\":true}]", SortOrder = 0, IsActive = false },
            new LoanProduct { Name = "اقساط ۳ ماهه", Slug = "p3", Description = "برای خریدهای کوچک و روزمره", Badge = "بدون پیش‌پرداخت", MinAmount = 1_000_000, MaxAmount = 20_000_000, TiersJson = T((3, 2, 0)), ConditionsJson = C, DocumentsJson = D, SortOrder = 1 },
            new LoanProduct { Name = "اقساط ۶ ماهه", Slug = "p6", Description = "متعادل‌ترین انتخاب", Badge = "پرطرفدار", MinAmount = 1_000_000, MaxAmount = 30_000_000, TiersJson = T((4, 2.3, 5), (6, 2.5, 10)), ConditionsJson = C, DocumentsJson = D, SortOrder = 2 },
            new LoanProduct { Name = "اقساط ۹ ماهه", Slug = "p9", Description = "برای خریدهای بزرگ‌تر", MinAmount = 3_000_000, MaxAmount = 40_000_000, TiersJson = T((9, 2.5, 15)), ConditionsJson = C, DocumentsJson = D, SortOrder = 3 });
        foreach (var e in _db.ChangeTracker.Entries<LoanProduct>().Where(e => e.State == EntityState.Added)) e.Entity.TenantId = this.Tid();
        await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ========== کارت مجازی: مشتری ==========
[ApiController, Route("api/v1/me/cards"), Authorize]
public class MyCardsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ICardCrypto _c; public MyCardsController(AppDbContext db, ICardCrypto c) { _db = db; _c = c; }
    [HttpGet] public async Task<IActionResult> Mine(CancellationToken ct)
    { var u = this.Uid(); if (u == null) return Unauthorized(); return Ok((await _db.Set<VirtualCard>().Where(c => c.TenantId == this.Tid() && c.UserId == u).OrderByDescending(c => c.CreatedOn).ToListAsync(ct)).Select(U.Dto)); }

    /// نمایش شماره‌ی کامل و CVV (هر بار در گردش کارت ثبت می‌شود).
    [HttpPost("{id:guid}/reveal")] public async Task<IActionResult> Reveal(Guid id, CancellationToken ct)
    {
        var u = this.Uid(); var c = await _db.Set<VirtualCard>().FirstOrDefaultAsync(x => x.TenantId == this.Tid() && x.Id == id && x.UserId == u, ct); if (c == null) return NotFound();
        if (c.Status == "Closed") return BadRequest(new { message = "کارت بسته شده است" });
        _db.Add(new VirtualCardTxn { CardId = c.Id, Type = "Reveal", Note = "نمایش اطلاعات کارت توسط مشتری" }); await _db.SaveChangesAsync(ct);
        return Ok(new { number = _c.Unprotect(c.PanEnc), cvv = _c.Unprotect(c.CvvEnc), expiry = $"{c.ExpYear}/{c.ExpMonth:00}" });
    }
    [HttpPost("{id:guid}/freeze")] public Task<IActionResult> Freeze(Guid id, CancellationToken ct) => Set(id, "Frozen", "Freeze", ct);
    [HttpPost("{id:guid}/unfreeze")] public Task<IActionResult> Unfreeze(Guid id, CancellationToken ct) => Set(id, "Active", "Unfreeze", ct);
    async Task<IActionResult> Set(Guid id, string st, string type, CancellationToken ct)
    {
        var u = this.Uid(); var c = await _db.Set<VirtualCard>().FirstOrDefaultAsync(x => x.TenantId == this.Tid() && x.Id == id && x.UserId == u, ct); if (c == null) return NotFound();
        if (c.Status == "Closed") return BadRequest(new { message = "کارت بسته شده است" });
        c.Status = st; _db.Add(new VirtualCardTxn { CardId = c.Id, Type = type }); await _db.SaveChangesAsync(ct); return Ok(U.Dto(c));
    }
    [HttpGet("{id:guid}/transactions")] public async Task<IActionResult> Txns(Guid id, CancellationToken ct)
    {
        var u = this.Uid(); if (!await _db.Set<VirtualCard>().AnyAsync(x => x.TenantId == this.Tid() && x.Id == id && x.UserId == u, ct)) return NotFound();
        return Ok(await _db.Set<VirtualCardTxn>().Where(t => t.CardId == id).OrderByDescending(t => t.CreatedOn).Take(100).ToListAsync(ct));
    }
}

// ========== کارت مجازی: مدیر ==========
public record IssueInput(Guid LoanId, Guid UserId, string HolderName, decimal Limit);
public record LimitInput(decimal Limit, decimal? PerTxnLimit);
[ApiController, Route("api/v1/admin/virtual-cards"), Authorize(Roles = "Admin,SuperAdmin")]
public class VirtualCardsAdminController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IVirtualCardService _s;
    public VirtualCardsAdminController(AppDbContext db, IVirtualCardService s) { _db = db; _s = s; }
    [HttpGet] public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    { var q = _db.Set<VirtualCard>().Where(c => c.TenantId == this.Tid()); if (!string.IsNullOrEmpty(status)) q = q.Where(c => c.Status == status); return Ok((await q.OrderByDescending(c => c.CreatedOn).Take(500).ToListAsync(ct)).Select(U.Dto)); }
    [HttpPost("issue")] public async Task<IActionResult> Issue(IssueInput i, CancellationToken ct)
    {
        if (i.Limit <= 0 || string.IsNullOrWhiteSpace(i.HolderName)) return BadRequest(new { message = "سقف و نام دارنده الزامی است" });
        return Ok(U.Dto(await _s.IssueForLoanAsync(i.LoanId, i.UserId, i.HolderName.Trim(), i.Limit, this.Tid(), ct)));
    }
    [HttpPost("{id:guid}/{op}")] public async Task<IActionResult> Act(Guid id, string op, CancellationToken ct)
    {
        var c = await _db.Set<VirtualCard>().FindAsync(new object[] { id }, ct); if (c == null || c.TenantId != this.Tid()) return NotFound();
        (string st, string type)? m = op switch { "freeze" => ("Frozen", "Freeze"), "unfreeze" => ("Active", "Unfreeze"), "close" => ("Closed", "Close"), _ => null };
        if (m == null) return NotFound(); if (c.Status == "Closed") return BadRequest(new { message = "کارت بسته شده است" });
        c.Status = m.Value.st; _db.Add(new VirtualCardTxn { CardId = c.Id, Type = m.Value.type, Note = "توسط مدیر" }); await _db.SaveChangesAsync(ct); return Ok(U.Dto(c));
    }
    [HttpPut("{id:guid}/limit")] public async Task<IActionResult> Limit(Guid id, LimitInput i, CancellationToken ct)
    {
        var c = await _db.Set<VirtualCard>().FindAsync(new object[] { id }, ct); if (c == null || c.TenantId != this.Tid()) return NotFound();
        if (i.Limit < c.Spent) return BadRequest(new { message = "سقف کمتر از مبلغ مصرف‌شده نمی‌شود" });
        c.Limit = i.Limit; if (i.PerTxnLimit is > 0) c.PerTxnLimit = i.PerTxnLimit.Value;
        _db.Add(new VirtualCardTxn { CardId = c.Id, Type = "LimitChange", Amount = c.Limit, Note = "توسط مدیر" }); await _db.SaveChangesAsync(ct); return Ok(U.Dto(c));
    }
    [HttpGet("{id:guid}/transactions")] public async Task<IActionResult> Txns(Guid id, CancellationToken ct)
    {
        if (!await _db.Set<VirtualCard>().AnyAsync(x => x.Id == id && x.TenantId == this.Tid(), ct)) return NotFound();
        return Ok(await _db.Set<VirtualCardTxn>().Where(t => t.CardId == id).OrderByDescending(t => t.CreatedOn).Take(200).ToListAsync(ct));
    }
}

// ========== درگاه داخلی: پرداخت با کارت مجازی ==========
public record ChargeInput(string CardNumber, string Cvv, string Expiry, decimal Amount, string OrderId, string? MerchantName);
[ApiController, Route("api/v1/gateway")]
public class GatewayController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ICardCrypto _c; private readonly IConfiguration _cfg; private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _mc;
    // سرور تک‌نمونه است: قفل هر کارت جلوی دو پرداخت هم‌زمان روی یک کارت را می‌گیرد.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();
    public GatewayController(AppDbContext db, ICardCrypto c, IConfiguration cfg, Microsoft.Extensions.Caching.Memory.IMemoryCache mc) { _db = db; _c = c; _cfg = cfg; _mc = mc; }
    bool Hit(string key, int max, TimeSpan w) { var n = _mc.GetOrCreate(key, e => { e.AbsoluteExpirationRelativeToNow = w; return new int[1]; })!; return Interlocked.Increment(ref n[0]) <= max; }
    bool Blocked(string key, int max) => _mc.TryGetValue(key, out int[]? n) && n != null && n[0] >= max;

    /// هدر X-Merchant-Key باید در appsettings (Gateway:MerchantKeys) تعریف شده باشد.
    [HttpPost("card-charge"), AllowAnonymous]
    public async Task<IActionResult> Charge(ChargeInput i, CancellationToken ct)
    {
        var keys = _cfg.GetSection("Gateway:MerchantKeys").Get<string[]>() ?? Array.Empty<string>();
        if (keys.Length == 0) return StatusCode(503, new { message = "کلید پذیرنده تنظیم نشده است" });
        if (!Hit("gwip:" + this.ClientIp(), 60, TimeSpan.FromMinutes(1))) return StatusCode(429, new { status = "failed", message = "درخواست‌ها بیش از حد مجاز است" });
        var given = System.Text.Encoding.UTF8.GetBytes(Request.Headers["X-Merchant-Key"].ToString());
        if (!keys.Any(k => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(k), given))) return Unauthorized(new { message = "کلید پذیرنده نامعتبر" });
        if (i.Amount <= 0 || string.IsNullOrWhiteSpace(i.OrderId)) return BadRequest(new { message = "مبلغ و شماره سفارش الزامی است" });
        var pan = new string((i.CardNumber ?? "").Where(char.IsDigit).ToArray());
        var ph = _c.Hash(pan); var failKey = "gwfail:" + ph;
        // پیام یکسان برای جلوگیری از حدس‌زدن کارت
        IActionResult Bad(string m = "کارت یا اطلاعات آن نامعتبر است") => BadRequest(new { status = "failed", message = m });
        if (Blocked(failKey, 5)) return StatusCode(429, new { status = "failed", message = "به دلیل تلاش‌های ناموفق، این کارت موقتاً مسدود است. بعداً تلاش کنید" });
        var c = await _db.Set<VirtualCard>().FirstOrDefaultAsync(x => x.PanHash == ph, ct);
        if (c == null || _c.Unprotect(c.CvvEnc) != (i.Cvv ?? "") || i.Expiry != $"{c.ExpYear}/{c.ExpMonth:00}") { Hit(failKey, 5, TimeSpan.FromMinutes(15)); return Bad(); }

        var sem = Locks.GetOrAdd(c.Id, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            await _db.Entry(c).ReloadAsync(ct);
            if (c.Status != "Active") return Bad("کارت فعال نیست");
            if (await _db.Set<VirtualCardTxn>().AnyAsync(t => t.CardId == c.Id && t.Reference == i.OrderId && t.Type == "Charge", ct)) return Conflict(new { status = "failed", message = "این سفارش قبلاً پرداخت شده" });
            if (i.Amount > c.PerTxnLimit) return Bad("مبلغ از سقف هر خرید بیشتر است");
            var amt = i.Amount;
            // کسر اتمیک در خود دیتابیس: حتی با خطای هم‌زمانی هم هرگز از سقف بیشتر نمی‌شود
            var n = await _db.Set<VirtualCard>().Where(x => x.Id == c.Id && x.Status == "Active" && x.Spent + amt <= x.Limit).ExecuteUpdateAsync(s => s.SetProperty(x => x.Spent, x => x.Spent + amt), ct);
            if (n == 0) return Bad("اعتبار کارت کافی نیست");
            var t = new VirtualCardTxn { CardId = c.Id, Type = "Charge", Amount = amt, Merchant = i.MerchantName ?? "", Reference = i.OrderId };
            try { _db.Add(t); await _db.SaveChangesAsync(ct); }
            catch { await _db.Set<VirtualCard>().Where(x => x.Id == c.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Spent, x => x.Spent - amt), CancellationToken.None); throw; }
            await _db.Entry(c).ReloadAsync(ct);
            return Ok(new { status = "paid", refId = "GH-" + t.Id.ToString("N")[..8].ToUpper(), amount = amt, remaining = c.Limit - c.Spent });
        }
        finally { sem.Release(); }
    }
}

// ========== زونکن دیجیتال ==========
[ApiController, Route("api/v1/me/documents"), Authorize]
public class MyDocumentsController : ControllerBase
{
    private static readonly string[] Allowed = { "image/jpeg", "image/png", "image/webp" };
    private readonly AppDbContext _db; private readonly ICardCrypto _c; private readonly IOcrProvider _ocr; private readonly IConfiguration _cfg;
    public MyDocumentsController(AppDbContext db, ICardCrypto c, IOcrProvider ocr, IConfiguration cfg) { _db = db; _c = c; _ocr = ocr; _cfg = cfg; }
    string Dir => _cfg["Documents:Path"] ?? "/home/ghestine/private-docs";

    [HttpGet] public async Task<IActionResult> Mine(CancellationToken ct)
    { var u = this.Uid(); if (u == null) return Unauthorized(); return Ok(await _db.Set<CustomerDocument>().Where(d => d.TenantId == this.Tid() && d.UserId == u).OrderByDescending(d => d.CreatedOn).Select(d => new { d.Id, d.Kind, d.LoanId, d.FileName, d.Status, d.ReviewNote, d.CreatedOn, hasText = d.OcrText != null }).ToListAsync(ct)); }

    [HttpPost, RequestSizeLimit(8_000_000)]
    public async Task<IActionResult> Upload([FromForm] string kind, [FromForm] Guid? loanId, IFormFile file, CancellationToken ct)
    {
        var u = this.Uid(); if (u == null) return Unauthorized();
        if (file == null || file.Length == 0) return BadRequest(new { message = "عکس مدرک الزامی است" });
        if (file.Length > 6_000_000) return BadRequest(new { message = "حجم عکس حداکثر ۶ مگابایت" });
        if (!Allowed.Contains(file.ContentType)) return BadRequest(new { message = "فقط عکس JPG، PNG یا WEBP" });
        if (string.IsNullOrWhiteSpace(kind)) return BadRequest(new { message = "نوع مدرک را مشخص کنید" });
        byte[] bytes; using (var ms = new MemoryStream()) { await file.CopyToAsync(ms, ct); bytes = ms.ToArray(); }
        if (!MagicOk(bytes, file.ContentType)) return BadRequest(new { message = "فایل عکس معتبر نیست" });
        Directory.CreateDirectory(Dir);
        var id = Guid.NewGuid(); var path = Path.Combine(Dir, id.ToString("N") + ".bin");
        await System.IO.File.WriteAllBytesAsync(path, _c.ProtectBytes(bytes), ct);
        var (text, fields) = await _ocr.ExtractAsync(bytes, file.ContentType, kind, ct);
        var d = new CustomerDocument { Id = id, TenantId = this.Tid(), UserId = u.Value, LoanId = loanId, Kind = kind.Trim(), FileName = Path.GetFileName(file.FileName), ContentType = file.ContentType, StoragePath = path, SizeBytes = file.Length, OcrText = text, FieldsJson = fields };
        _db.Add(d); await _db.SaveChangesAsync(ct);
        return Ok(new { d.Id, d.Status, ocr = text != null });
    }
    static bool MagicOk(byte[] b, string ct) => ct switch
    {
        "image/jpeg" => b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8,
        "image/png" => b.Length > 8 && b[0] == 0x89 && b[1] == 0x50,
        "image/webp" => b.Length > 12 && b[0] == 0x52 && b[8] == 0x57,
        _ => false
    };
}

public record ReviewInput(string Status, string? Note, string? OcrText);
[ApiController, Route("api/v1/admin/documents"), Authorize(Roles = "Admin,SuperAdmin")]
public class DocumentsAdminController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ICardCrypto _c; public DocumentsAdminController(AppDbContext db, ICardCrypto c) { _db = db; _c = c; }
    [HttpGet] public async Task<IActionResult> List([FromQuery] Guid? userId, [FromQuery] string? status, [FromQuery] string? kind, CancellationToken ct)
    {
        var q = _db.Set<CustomerDocument>().Where(d => d.TenantId == this.Tid());
        if (userId != null) q = q.Where(d => d.UserId == userId); if (!string.IsNullOrEmpty(status)) q = q.Where(d => d.Status == status); if (!string.IsNullOrEmpty(kind)) q = q.Where(d => d.Kind == kind);
        return Ok(await q.OrderByDescending(d => d.CreatedOn).Take(500).Select(d => new { d.Id, d.UserId, d.LoanId, d.Kind, d.FileName, d.Status, d.ReviewNote, d.OcrText, d.FieldsJson, d.SizeBytes, d.CreatedOn }).ToListAsync(ct));
    }
    [HttpGet("{id:guid}/file")] public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var d = await _db.Set<CustomerDocument>().FindAsync(new object[] { id }, ct); if (d == null || d.TenantId != this.Tid() || !System.IO.File.Exists(d.StoragePath)) return NotFound();
        return base.File(_c.UnprotectBytes(await System.IO.File.ReadAllBytesAsync(d.StoragePath, ct)), d.ContentType);
    }
    [HttpPut("{id:guid}/review")] public async Task<IActionResult> Review(Guid id, ReviewInput i, CancellationToken ct)
    {
        if (i.Status is not ("Verified" or "Rejected" or "Pending")) return BadRequest(new { message = "وضعیت نامعتبر" });
        var d = await _db.Set<CustomerDocument>().FindAsync(new object[] { id }, ct); if (d == null || d.TenantId != this.Tid()) return NotFound();
        d.Status = i.Status; d.ReviewNote = i.Note; if (i.OcrText != null) d.OcrText = i.OcrText; await _db.SaveChangesAsync(ct); return Ok();
    }
}

// ========== طرح بتا: استعلام از API بانک (منتظر مستندات واقعی) ==========
public record BetaInquiryInput(string NationalCode, bool Consent);
[ApiController, Route("api/v1/beta"), Authorize]
public class BetaController : ControllerBase
{
    [HttpPost("inquiry")]
    public IActionResult Inquiry(BetaInquiryInput i)
    {
        if (!i.Consent) return BadRequest(new { message = "رضایت استعلام لازم است" });
        if (!Iran.NationalCodeOk(i.NationalCode ?? "")) return BadRequest(new { message = "کد ملی از نظر ساختار معتبر نیست" });
        // اتصال واقعی بعد از دریافت مستندات API بانک رفاه نوشته می‌شود. تا آن موقع پاسخ ساختگی برنمی‌گردد.
        return StatusCode(501, new { message = "اتصال به سامانه بانک هنوز فعال نشده است" });
    }
}
