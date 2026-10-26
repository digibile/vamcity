using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ghestine.API.Tenancy;
namespace Ghestine.API.Security;

public record OtpRequestDto(string Phone, string CaptchaId, string CaptchaAnswer, string? Purpose);
public record OtpVerifyDto(string Phone, string Code, string? Purpose);
public record LoginDto(string Username, string Password, string CaptchaId, string CaptchaAnswer);
public record RegisterDto(string Ticket, string FullName, string NationalCode, string Password, string? Email, string? ReferralCode, string CaptchaId, string CaptchaAnswer);

[ApiController, Route("api/v1/security"), AllowAnonymous]
public class SecurityController : ControllerBase
{
    private readonly ICaptchaService _cap; private readonly IOtpService _otp; private readonly AuthAdapter _auth; private readonly IAuthTokenIssuer _iss; private readonly IRateLimiter _rl; private readonly IConfiguration _cfg;
    public SecurityController(ICaptchaService cap, IOtpService otp, AuthAdapter auth, IAuthTokenIssuer iss, IRateLimiter rl, IConfiguration cfg) { _cap = cap; _otp = otp; _auth = auth; _iss = iss; _rl = rl; _cfg = cfg; }
    string Ip
    {
        get
        {
            var ra = HttpContext.Connection.RemoteIpAddress;
            // پشت nginx آدرس واقعی فقط وقتی معتبر است که درخواست از خود سرور (loopback) آمده باشد.
            if (ra != null && System.Net.IPAddress.IsLoopback(ra))
            {
                var xf = Request.Headers["X-Real-IP"].FirstOrDefault() ?? Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(xf) && System.Net.IPAddress.TryParse(xf, out var ip)) return ip.ToString();
            }
            return ra?.ToString() ?? "unknown";
        }
    }
    string Pepper => Pep.Get(_cfg);

    [HttpGet("captcha")]
    public IActionResult Captcha() { Response.Headers.CacheControl = "no-store"; var c = _cap.Create(); return Ok(new { id = c.Id, image = c.Svg, expiresIn = c.ExpiresIn }); }

    [HttpPost("otp/request")]
    public async Task<IActionResult> RequestOtp(OtpRequestDto d, CancellationToken ct)
    { var r = await _otp.RequestAsync(d.Phone, Ip, d.CaptchaId, d.CaptchaAnswer, this.Brand(), d.Purpose ?? "login", ct); return StatusCode(r.Status, new { ok = r.Ok, message = r.Message, expiresIn = r.ExpiresIn, resendIn = r.ResendIn }); }

    /// وضعیت واقعی تایمر از سرور (با رفرش صفحه هم درست می‌ماند).
    [HttpGet("otp/status")]
    public async Task<IActionResult> Status([FromQuery] string phone, CancellationToken ct)
    { var p = OtpService.NormPhone(phone); if (p == "") return BadRequest(new { message = "شماره معتبر نیست" }); var r = await _otp.StatusAsync(p, ct); return Ok(new { expiresIn = r.ExpiresIn, resendIn = r.ResendIn }); }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> Verify(OtpVerifyDto d, CancellationToken ct)
    {
        var purpose = d.Purpose == "register" ? "register" : "login";
        var r = await _otp.VerifyAsync(d.Phone, d.Code, Ip, purpose, ct);
        if (!r.Ok) return StatusCode(r.Status, new { ok = false, message = r.Message });
        var phone = OtpService.NormPhone(d.Phone);
        if (purpose == "register") return Ok(new { ticket = Ticket.Make(phone, Pepper) });
        var auth = await _iss.IssueForPhoneAsync(phone, ct);
        return auth == null ? NotFound(new { ok = false, message = "حسابی با این شماره پیدا نشد یا غیرفعال است. ابتدا ثبت‌نام کنید" }) : Ok(auth);
    }

    /// ورود با رمز ثابت: کپچا + محدودیت تلاش + همان جدول کاربران و همان تنظیمات JWT پروژه.
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto d, CancellationToken ct)
    {
        const string bad = "نام کاربری یا رمز عبور نادرست است";
        if (!_rl.Hit("lgip:" + Ip, 40, TimeSpan.FromMinutes(15))) return StatusCode(429, new { message = "تلاش‌های ناموفق زیاد بود. کمی بعد دوباره امتحان کنید" });
        if (!_cap.Validate(d.CaptchaId, d.CaptchaAnswer)) return BadRequest(new { message = "کد امنیتی نادرست یا منقضی است" });
        var uname = (d.Username ?? "").Trim(); if (uname == "" || string.IsNullOrEmpty(d.Password)) return BadRequest(new { message = "نام کاربری و رمز را وارد کنید" });
        if (!_rl.Hit("lgu:" + uname.ToLowerInvariant(), 8, TimeSpan.FromMinutes(15))) return StatusCode(429, new { message = "برای این حساب تلاش‌های زیادی شده. ۱۵ دقیقه بعد دوباره امتحان کنید" });
        if (_auth.Map() == null) return StatusCode(503, new { message = "سامانه‌ی ورود هنوز پیکربندی نشده است" });
        var phone = OtpService.NormPhone(uname);
        var u = phone != "" ? await _auth.FindByPhoneAsync(phone, ct) : await _auth.FindByUsernameAsync(uname, ct);
        bool ok; try { ok = u != null && _auth.IsActive(u) && _auth.VerifyPassword(u, d.Password); } catch (InvalidOperationException) { return StatusCode(503, new { message = "بررسی رمز روی این سرور پیکربندی نشده است" }); }
        if (!ok) return Unauthorized(new { message = bad });
        return Ok(_auth.IssueToken(u!));
    }

    /// ثبت‌نام: نیازمند تیکت تأیید موبایل (OTP با purpose=register) و کپچا.
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto d, CancellationToken ct)
    {
        if (!_rl.Hit("rgip:" + Ip, 10, TimeSpan.FromHours(1))) return StatusCode(429, new { message = "تعداد ثبت‌نام از این شبکه زیاد است" });
        if (!_cap.Validate(d.CaptchaId, d.CaptchaAnswer)) return BadRequest(new { message = "کد امنیتی نادرست یا منقضی است" });
        var phone = Ticket.Check(d.Ticket, Pepper); if (phone == null) return BadRequest(new { message = "تأیید موبایل منقضی شده است. دوباره کد بگیرید" });
        var nid = new string((d.NationalCode ?? "").Select(c => c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c).Where(char.IsDigit).ToArray());
        if ((d.FullName ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2) return BadRequest(new { message = "نام و نام خانوادگی را کامل بنویسید" });
        if (!NationalCodeOk(nid)) return BadRequest(new { message = "کد ملی از نظر ساختار معتبر نیست" });
        if ((d.Password ?? "").Length < 8) return BadRequest(new { message = "رمز عبور حداقل ۸ نویسه باشد" });
        var (u, err) = await _auth.CreateUserAsync(d.FullName!.Trim(), phone, d.Email?.Trim(), nid, d.Password!, _cfg["Auth:CustomerRole"] ?? "Customer", this.Tid() == Guid.Empty ? null : this.Tid(), false, ct);
        if (u == null) return Conflict(new { message = err ?? "ثبت‌نام انجام نشد" });
        return Ok(_auth.IssueToken(u));
    }
    static bool NationalCodeOk(string v)
    {
        if (v.Length != 10 || v.Distinct().Count() == 1) return false;
        int s = 0; for (int i = 0; i < 9; i++) s += (v[i] - '0') * (10 - i);
        int r = s % 11, c = v[9] - '0'; return r < 2 ? c == r : c == 11 - r;
    }
}
