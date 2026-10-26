using System.Net;
using System.Security.Cryptography;
using System.Text;
using Ghestine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using __DBNS__;
namespace Ghestine.API.Security;

/// کلید محرمانه‌ی HMAC: اگر تنظیم نشده باشد، به‌جای مقدار پیش‌فرض ناامن، با خطا متوقف می‌شود.
public static class Pep
{
    public static string Get(IConfiguration c)
    {
        var v = c["OTP_PEPPER"];
        if (string.IsNullOrWhiteSpace(v) || v.Length < 24 || v == "change-me")
            throw new InvalidOperationException("OTP_PEPPER تنظیم نشده یا کوتاه‌تر از ۲۴ نویسه است (در /etc/ghestine/secrets.env).");
        return v;
    }
}

// ---------- کپچای سمت سرور (SVG، بدون وابستگی خارجی و بدون نیاز به اینترنت بین‌الملل) ----------
public interface ICaptchaService
{
    (string Id, string Svg, int ExpiresIn) Create();
    /// پاسخ را بررسی می‌کند و در هر حالت همان کپچا را مصرف می‌کند (استفاده‌ی دوباره ممکن نیست).
    bool Validate(string? id, string? answer);
}
public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _c; private readonly byte[] _pepper; private const int Ttl = 120;
    public CaptchaService(IMemoryCache c, IConfiguration cfg) { _c = c; _pepper = Encoding.UTF8.GetBytes(Pep.Get(cfg)); }
    string H(string id, string a) => Convert.ToHexString(new HMACSHA256(_pepper).ComputeHash(Encoding.UTF8.GetBytes(id + ":" + a)));
    public (string Id, string Svg, int ExpiresIn) Create()
    {
        var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
        var ans = string.Concat(Enumerable.Range(0, 5).Select(_ => RandomNumberGenerator.GetInt32(10)));
        _c.Set("cap:" + id, H(id, ans), TimeSpan.FromSeconds(Ttl));
        var sb = new StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' width='170' height='56' viewBox='0 0 170 56'><rect width='170' height='56' rx='12' fill='#241047'/>");
        for (int i = 0; i < 7; i++) sb.Append($"<path d='M{RandomNumberGenerator.GetInt32(170)} {RandomNumberGenerator.GetInt32(56)} Q{RandomNumberGenerator.GetInt32(170)} {RandomNumberGenerator.GetInt32(56)} {RandomNumberGenerator.GetInt32(170)} {RandomNumberGenerator.GetInt32(56)}' stroke='#{RandomNumberGenerator.GetInt32(0x555555, 0xAAAAAA):X6}' stroke-width='1.4' fill='none'/>");
        for (int i = 0; i < ans.Length; i++)
        {
            var x = 18 + i * 29; var y = 36 + RandomNumberGenerator.GetInt32(-5, 6); var r = RandomNumberGenerator.GetInt32(-24, 25);
            sb.Append($"<text x='{x}' y='{y}' transform='rotate({r} {x} {y})' font-family='monospace' font-size='{RandomNumberGenerator.GetInt32(26, 34)}' font-weight='700' fill='#{RandomNumberGenerator.GetInt32(0xD0D0D0, 0xFFFFFF):X6}'>{ans[i]}</text>");
        }
        sb.Append("</svg>");
        return (id, "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())), Ttl);
    }
    public bool Validate(string? id, string? answer)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(answer)) return false;
        if (!_c.TryGetValue("cap:" + id, out string? h) || h == null) return false;
        _c.Remove("cap:" + id);
        var a = new string(answer.Select(ch => ch >= '۰' && ch <= '۹' ? (char)('0' + ch - '۰') : ch).Where(char.IsDigit).ToArray());
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(h), Encoding.ASCII.GetBytes(H(id, a)));
    }
}

// ---------- محدودکننده‌ی درخواست ----------
public interface IRateLimiter { bool Hit(string key, int max, TimeSpan window); }
public class RateLimiter : IRateLimiter
{
    private readonly IMemoryCache _c; public RateLimiter(IMemoryCache c) => _c = c;
    private readonly object _l = new();
    public bool Hit(string key, int max, TimeSpan window)
    {
        lock (_l)
        {
            var list = _c.GetOrCreate("rl:" + key, e => { e.AbsoluteExpirationRelativeToNow = window; return new List<DateTime>(); })!;
            list.RemoveAll(t => t < DateTime.UtcNow - window);
            if (list.Count >= max) return false;
            list.Add(DateTime.UtcNow); return true;
        }
    }
}

// ---------- کاوه‌نگار ----------
public interface ISmsSender { Task<(bool Ok, string Info)> SendAsync(string phone, string text, CancellationToken ct); }
public class KavenegarSender : ISmsSender
{
    private readonly HttpClient _h; private readonly IConfiguration _cfg; private readonly ILogger<KavenegarSender> _log;
    public KavenegarSender(HttpClient h, IConfiguration cfg, ILogger<KavenegarSender> log) { _h = h; _cfg = cfg; _log = log; _h.Timeout = TimeSpan.FromSeconds(10); }
    public async Task<(bool Ok, string Info)> SendAsync(string phone, string text, CancellationToken ct)
    {
        var key = _cfg["KAVENEGAR_API_KEY"]; var sender = _cfg["KAVENEGAR_SENDER"];
        if (string.IsNullOrWhiteSpace(key) || key == "YOUR_API_KEY") return (false, "کلید کاوه‌نگار تنظیم نشده");
        var url = $"https://api.kavenegar.com/v1/{key}/sms/send.json";
        var form = new Dictionary<string, string> { ["receptor"] = phone, ["message"] = text };
        if (!string.IsNullOrWhiteSpace(sender)) form["sender"] = sender;
        try
        {
            using var r = await _h.PostAsync(url, new FormUrlEncodedContent(form), ct);
            var body = await r.Content.ReadAsStringAsync(ct);
            // در لاگ فقط کد وضعیت ثبت می‌شود؛ نه کلید، نه متن پیام.
            _log.LogInformation("Kavenegar HTTP {Status}", (int)r.StatusCode);
            if (r.StatusCode == HttpStatusCode.OK) return (true, "HTTP 200");
            return (false, "HTTP " + (int)r.StatusCode);
        }
        catch (Exception e) { _log.LogWarning("Kavenegar failed: {Type}", e.GetType().Name); return (false, e.GetType().Name); }
    }
}

// ---------- صدور توکن: به سیستم احراز هویت موجود وصل می‌شود ----------
public interface IAuthTokenIssuer
{
    /// اگر کاربری با این شماره هست، همان توکن/پاسخ ورود موجود پروژه را برگردان؛ وگرنه null.
    Task<object?> IssueForPhoneAsync(string phone, CancellationToken ct);
}
public class NotConfiguredTokenIssuer : IAuthTokenIssuer
{
    public Task<object?> IssueForPhoneAsync(string phone, CancellationToken ct) => Task.FromResult<object?>(null);
}

// ---------- OTP ----------
public record OtpResult(bool Ok, int Status, string Message, int ExpiresIn = 0, int ResendIn = 0);
public interface IOtpService
{
    Task<OtpResult> RequestAsync(string phone, string ip, string? capId, string? capAns, string brand, string purpose, CancellationToken ct);
    Task<OtpResult> VerifyAsync(string phone, string code, string ip, string purpose, CancellationToken ct);
    Task<OtpResult> StatusAsync(string phone, CancellationToken ct);
}
public class OtpService : IOtpService
{
    private readonly AppDbContext _db; private readonly ICaptchaService _cap; private readonly IRateLimiter _rl; private readonly ISmsSender _sms; private readonly IAuthTokenIssuer _iss; private readonly byte[] _pepper; private readonly int _ttl, _resend;
    public OtpService(AppDbContext db, ICaptchaService cap, IRateLimiter rl, ISmsSender sms, IAuthTokenIssuer iss, IConfiguration cfg)
    { _db = db; _cap = cap; _rl = rl; _sms = sms; _iss = iss; _pepper = Encoding.UTF8.GetBytes(Pep.Get(cfg));
      _ttl = int.TryParse(cfg["OTP_EXPIRES_SECONDS"], out var a) ? a : 120; _resend = int.TryParse(cfg["OTP_RESEND_SECONDS"], out var b) ? b : 120; }
    string H(string phone, string code) => Convert.ToHexString(new HMACSHA256(_pepper).ComputeHash(Encoding.UTF8.GetBytes(phone + ":" + code)));
    public static string NormPhone(string? p) { var s = new string((p ?? "").Select(c => c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c).Where(char.IsDigit).ToArray()); return System.Text.RegularExpressions.Regex.IsMatch(s, "^09\\d{9}$") ? s : ""; }

    public async Task<OtpResult> StatusAsync(string phone, CancellationToken ct)
    {
        var o = await _db.Set<OtpChallenge>().Where(x => x.Phone == phone && x.ConsumedAt == null).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        var now = DateTime.UtcNow;
        if (o == null) return new OtpResult(true, 200, "کدی فعال نیست");
        return new OtpResult(true, 200, "ok", Math.Max(0, (int)(o.ExpiresAt - now).TotalSeconds), Math.Max(0, (int)(o.ResendAt - now).TotalSeconds));
    }
    public async Task<OtpResult> RequestAsync(string phone, string ip, string? capId, string? capAns, string brand, string purpose, CancellationToken ct)
    {
        phone = NormPhone(phone); if (phone == "") return new(false, 400, "شماره موبایل معتبر نیست");
        if (!_rl.Hit("otpip:" + ip, 20, TimeSpan.FromHours(1))) return new(false, 429, "تعداد درخواست از این شبکه زیاد است. بعداً تلاش کنید");
        if (!_cap.Validate(capId, capAns)) return new(false, 400, "کد امنیتی نادرست یا منقضی است");
        var now = DateTime.UtcNow;
        var last = await _db.Set<OtpChallenge>().Where(x => x.Phone == phone && x.Purpose == (purpose == "register" ? "register" : "login")).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (last != null && last.ResendAt > now) return new(false, 429, "ارسال مجدد هنوز ممکن نیست", Math.Max(0, (int)(last.ExpiresAt - now).TotalSeconds), (int)Math.Ceiling((last.ResendAt - now).TotalSeconds));
        if (!_rl.Hit("otpph:" + phone, 5, TimeSpan.FromHours(1))) return new(false, 429, "برای این شماره در یک ساعت گذشته بیش از حد درخواست شده");
        // کدهای قبلی منقضی می‌شوند
        var pp = purpose == "register" ? "register" : "login"; foreach (var o in await _db.Set<OtpChallenge>().Where(x => x.Phone == phone && x.Purpose == pp && x.ConsumedAt == null).ToListAsync(ct)) o.ConsumedAt = now;
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var ch = new OtpChallenge { Id = Guid.NewGuid(), Purpose = purpose == "register" ? "register" : "login", Phone = phone, Ip = ip, CodeHash = H(phone, code), ExpiresAt = now.AddSeconds(_ttl), ResendAt = now.AddSeconds(_resend), CreatedAt = now };
        _db.Add(ch); await _db.SaveChangesAsync(ct);
        var (ok, info) = await _sms.SendAsync(phone, $"کد ورود {brand}: {code}\nاین کد را به کسی ندهید.", ct);
        ch.SendStatus = ok ? "Sent" : "Failed"; ch.SendInfo = info;
        if (!ok) { ch.ConsumedAt = DateTime.UtcNow; ch.ResendAt = now; }
        await _db.SaveChangesAsync(ct);
        return ok ? new(true, 200, "کد ارسال شد", _ttl, _resend) : new(false, 502, "ارسال پیامک انجام نشد. کمی بعد دوباره تلاش کنید");
    }
    public async Task<OtpResult> VerifyAsync(string phone, string code, string ip, string purpose, CancellationToken ct)
    {
        phone = NormPhone(phone); code = new string((code ?? "").Select(c => c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c).Where(char.IsDigit).ToArray());
        purpose = purpose == "register" ? "register" : "login";
        if (phone == "" || code.Length != 6) return new(false, 400, "کد را کامل وارد کنید");
        if (!_rl.Hit("otpv:" + ip, 40, TimeSpan.FromMinutes(15))) return new(false, 429, "تلاش‌های زیاد. کمی بعد دوباره امتحان کنید");
        var now = DateTime.UtcNow;
        var o = await _db.Set<OtpChallenge>().Where(x => x.Phone == phone && x.Purpose == purpose && x.ConsumedAt == null).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(ct);
        if (o == null || o.ExpiresAt < now) return new(false, 400, "کد منقضی شده است. کد جدید بگیرید");
        if (o.Attempts >= 5) { o.ConsumedAt = now; await _db.SaveChangesAsync(ct); return new(false, 429, "تعداد تلاش بیش از حد. کد جدید بگیرید"); }
        o.Attempts++;
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(o.CodeHash), Encoding.ASCII.GetBytes(H(phone, code)))) { await _db.SaveChangesAsync(ct); return new(false, 400, "کد نادرست است"); }
        o.ConsumedAt = now; await _db.SaveChangesAsync(ct);
        return new(true, 200, "تأیید شد");
    }
}
