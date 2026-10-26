using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.IdentityModel.Tokens;
using __DBNS__;
namespace Ghestine.API.Security;

/// نقشه‌ی موجودیت کاربر در پروژه. به‌جای حدس زدن، از مدل EF خوانده می‌شود و اگر مبهم بود هیچ چیزی نوشته نمی‌شود.
public class UserMap
{
    public Type Clr = null!; public PropertyInfo? Id, Phone, Hash, Name, First, Last, Email, Nid, Role, Active, MustChange, Username, Tenant, Created;
    public List<string> Notes = new();
}

public class AuthAdapter
{
    private readonly AppDbContext _db; private readonly IConfiguration _cfg;
    public AuthAdapter(AppDbContext db, IConfiguration cfg) { _db = db; _cfg = cfg; }
    const BindingFlags BF = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
    static UserMap? _map; static string? _why;
    public string? WhyNot => _why;

    static PropertyInfo? P(Type t, params string[] names) { foreach (var n in names) { var p = t.GetProperty(n, BF); if (p != null && p.CanRead && p.CanWrite) return p; } return null; }

    public UserMap? Map()
    {
        if (_map != null) return _map;
        var cands = new List<UserMap>();
        foreach (var et in _db.Model.GetEntityTypes())
        {
            var t = et.ClrType; if (t.IsAbstract || et.IsOwned()) continue;
            var phone = P(t, "PhoneNumber", "Mobile", "MobileNumber", "Phone", "CellPhone"); var hash = P(t, "PasswordHash", "HashedPassword", "Password");
            if (phone == null || hash == null) continue;
            cands.Add(new UserMap { Clr = t, Phone = phone, Hash = hash, Id = P(t, "Id"), Name = P(t, "FullName", "DisplayName", "Name"), First = P(t, "FirstName"), Last = P(t, "LastName"),
                Email = P(t, "Email"), Nid = P(t, "NationalCode", "NationalId", "NationalNumber"), Role = P(t, "Role", "RoleName", "UserRole"), Active = P(t, "IsActive", "Active", "Enabled"),
                MustChange = P(t, "MustChangePassword", "ForcePasswordChange", "RequirePasswordChange"), Username = P(t, "UserName", "Username"), Tenant = P(t, "TenantId"), Created = P(t, "CreatedOn", "CreatedAt", "CreatedDate") });
        }
        UserMap? pick = null;
        if (cands.Count == 1) pick = cands[0];
        else if (cands.Count > 1)
        {
            var pref = cands.Where(c => System.Text.RegularExpressions.Regex.IsMatch(c.Clr.Name, "^(ApplicationUser|AppUser|User|Users)$")).ToList();
            if (pref.Count == 1) pick = pref[0]; else _why = "چند موجودیت شبیه کاربر پیدا شد: " + string.Join("، ", cands.Select(c => c.Clr.Name));
        }
        else _why = "موجودیتی با ستون شماره موبایل و هش رمز پیدا نشد";
        if (pick != null)
        {
            if (pick.Id == null) pick.Notes.Add("ستون Id پیدا نشد");
            if (pick.Phone!.PropertyType != typeof(string)) { _why = "ستون موبایل از نوع رشته نیست"; pick = null; }
            else if (pick.Hash!.PropertyType != typeof(string)) { _why = "ستون هش رمز از نوع رشته نیست"; pick = null; }
            else
            {
                if (pick.Role == null) pick.Notes.Add("ستون نقش ساده (رشته یا enum) پیدا نشد؛ نقش‌های جدولی/چندگانه پشتیبانی نمی‌شود");
                else if (pick.Role.PropertyType != typeof(string) && !pick.Role.PropertyType.IsEnum) { pick.Notes.Add("نوع ستون نقش پشتیبانی نمی‌شود"); pick.Role = null; }
                if (pick.Name == null && (pick.First == null || pick.Last == null)) pick.Notes.Add("ستون نام پیدا نشد");
            }
        }
        _map = pick; return pick;
    }

    IQueryable Set(Type t) => (IQueryable)typeof(DbContext).GetMethods().First(m => m.Name == "Set" && m.IsGenericMethod && m.GetParameters().Length == 0).MakeGenericMethod(t).Invoke(_db, null)!;
    static async Task<object?> FirstAsync(IQueryable q, Type t, CancellationToken ct)
    {
        var mi = typeof(EntityFrameworkQueryableExtensions).GetMethods().First(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(CancellationToken)).MakeGenericMethod(t);
        var task = (Task)mi.Invoke(null, new object[] { q, ct })!; await task; return task.GetType().GetProperty("Result")!.GetValue(task);
    }
    IQueryable WhereIn(UserMap m, PropertyInfo prop, string[] values)
    {
        var p = Expression.Parameter(m.Clr, "e");
        var access = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(string) }, p, Expression.Constant(prop.Name));
        var contains = Expression.Call(typeof(Enumerable), "Contains", new[] { typeof(string) }, Expression.Constant(values), access);
        var lambda = Expression.Lambda(contains, p);
        var where = typeof(Queryable).GetMethods().First(x => x.Name == "Where" && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2).MakeGenericMethod(m.Clr);
        return (IQueryable)where.Invoke(null, new object[] { Set(m.Clr), lambda })!;
    }
    public static string[] PhoneForms(string p09) => new[] { p09, "98" + p09[1..], "+98" + p09[1..], p09[1..] };
    public async Task<object?> FindByPhoneAsync(string p09, CancellationToken ct) { var m = Map(); return m == null ? null : await FirstAsync(WhereIn(m, m.Phone!, PhoneForms(p09)), m.Clr, ct); }
    public async Task<object?> FindByUsernameAsync(string u, CancellationToken ct) { var m = Map(); if (m?.Username == null) return null; return await FirstAsync(WhereIn(m, m.Username, new[] { u }), m.Clr, ct); }
    public async Task<string?> SampleHashAsync(CancellationToken ct)
    {
        var m = Map(); if (m == null) return null;
        var p = Expression.Parameter(m.Clr, "e"); var acc = Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(string) }, p, Expression.Constant(m.Hash!.Name));
        var nn = Expression.NotEqual(acc, Expression.Constant(null, typeof(string)));
        var where = typeof(Queryable).GetMethods().First(x => x.Name == "Where" && x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2).MakeGenericMethod(m.Clr);
        var q = (IQueryable)where.Invoke(null, new object[] { Set(m.Clr), Expression.Lambda(nn, p) })!;
        var u = await FirstAsync(q, m.Clr, ct); return u == null ? null : (string?)m.Hash.GetValue(u);
    }

    public bool IsActive(object user) { var m = Map()!; return m.Active == null || m.Active.PropertyType != typeof(bool) || (bool)m.Active.GetValue(user)!; }
    public string? RoleOf(object user) { var m = Map()!; return m.Role?.GetValue(user)?.ToString(); }
    public string DisplayName(object user) { var m = Map()!; var n = m.Name?.GetValue(user) as string; if (!string.IsNullOrWhiteSpace(n)) return n!; return ((m.First?.GetValue(user) as string) + " " + (m.Last?.GetValue(user) as string)).Trim(); }
    public bool VerifyPassword(object user, string password) => PasswordHelper.Verify((string?)Map()!.Hash!.GetValue(user), password);

    // ---------- صدور توکن با همان تنظیمات JWT پروژه ----------
    string? Cfg(params string[] keys) { foreach (var k in keys) { var v = _cfg[k]; if (!string.IsNullOrWhiteSpace(v)) return v; } return null; }
    public object IssueToken(object user)
    {
        var m = Map()!; var key = Cfg("Jwt:Key", "Jwt:Secret", "JwtSettings:Key", "JwtSettings:Secret", "Authentication:Jwt:Key", "Jwt:SecretKey")
            ?? throw new InvalidOperationException("کلید JWT در تنظیمات پیدا نشد");
        var issuer = Cfg("Jwt:Issuer", "JwtSettings:Issuer", "Authentication:Jwt:Issuer"); var aud = Cfg("Jwt:Audience", "JwtSettings:Audience", "Authentication:Jwt:Audience");
        var id = m.Id?.GetValue(user)?.ToString() ?? "";
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, id), new(JwtRegisteredClaimNames.Sub, id), new(ClaimTypes.Name, DisplayName(user)), new(ClaimTypes.MobilePhone, (string?)m.Phone!.GetValue(user) ?? "") };
        var role = RoleOf(user); if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimTypes.Role, role));
        // کنترلرهای قدیمی معمولاً فقط [Authorize(Roles="Admin")] دارند؛ سوپرادمین باید از آن‌ها هم عبور کند.
        if (role == "SuperAdmin") claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        var minutes = int.TryParse(Cfg("Jwt:ExpiresMinutes", "Jwt:ExpireMinutes", "JwtSettings:ExpiryMinutes"), out var mm) ? mm : 120;
        var exp = DateTime.UtcNow.AddMinutes(minutes);
        var jwt = new JwtSecurityToken(issuer, aud, claims, DateTime.UtcNow.AddSeconds(-5), exp, new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));
        var tok = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new { accessToken = tok, token = tok, expiresAt = exp, user = new { id, name = DisplayName(user), role } };
    }

    // ---------- ساخت کاربر با ستون‌های واقعی ----------
    public async Task<(object? User, string? Error)> CreateUserAsync(string name, string phone09, string? email, string? nid, string password, string role, Guid? tenant, bool mustChange, CancellationToken ct)
    {
        var m = Map(); if (m == null) return (null, WhyNot);
        if (await FindByPhoneAsync(phone09, ct) != null) return (null, "کاربری با این شماره از قبل هست");
        var sample = await SampleHashAsync(ct);
        string hash; try { hash = PasswordHelper.Hash(password, sample); } catch (Exception e) { return (null, e.Message); }
        var u = Activator.CreateInstance(m.Clr)!;
        if (m.Id != null && m.Id.PropertyType == typeof(Guid)) m.Id.SetValue(u, Guid.NewGuid());
        m.Phone!.SetValue(u, phone09); m.Hash!.SetValue(u, hash);
        if (m.Name != null) m.Name.SetValue(u, name); else if (m.First != null && m.Last != null) { var parts = name.Split(' ', 2); m.First.SetValue(u, parts[0]); m.Last.SetValue(u, parts.Length > 1 ? parts[1] : parts[0]); }
        if (m.Email != null && !string.IsNullOrWhiteSpace(email)) m.Email.SetValue(u, email);
        if (m.Username != null && m.Username.PropertyType == typeof(string) && m.Username.GetValue(u) == null) m.Username.SetValue(u, phone09);
        if (m.Nid != null && !string.IsNullOrWhiteSpace(nid) && m.Nid.PropertyType == typeof(string)) m.Nid.SetValue(u, nid);
        if (m.Role != null)
        {
            if (m.Role.PropertyType.IsEnum) { if (!Enum.TryParse(m.Role.PropertyType, role, true, out var ev)) return (null, $"مقدار نقش «{role}» در enum وجود ندارد"); m.Role.SetValue(u, ev); }
            else m.Role.SetValue(u, role);
        }
        else return (null, "ستون نقش ساده پیدا نشد؛ ساخت کاربر ممکن نیست");
        if (m.Active != null && m.Active.PropertyType == typeof(bool)) m.Active.SetValue(u, true);
        if (m.MustChange != null && m.MustChange.PropertyType == typeof(bool)) m.MustChange.SetValue(u, mustChange);
        if (m.Tenant != null && tenant != null && m.Tenant.PropertyType == typeof(Guid)) m.Tenant.SetValue(u, tenant.Value);
        if (m.Created != null && m.Created.PropertyType == typeof(DateTime)) m.Created.SetValue(u, DateTime.UtcNow);
        _db.Add(u); await _db.SaveChangesAsync(ct); return (u, null);
    }
}

// ---------- هش و بررسی رمز: همان قالب نمونه‌ی موجود در پایگاه داده ----------
public static class PasswordHelper
{
    static Type? Bcrypt() => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("BCrypt.Net.BCrypt", false)).FirstOrDefault(t => t != null);
    public static bool Verify(string? stored, string password)
    {
        if (string.IsNullOrEmpty(stored)) return false;
        try
        {
            if (stored.StartsWith("$2"))
            {
                var b = Bcrypt() ?? throw new InvalidOperationException("BCrypt بارگذاری نشده");
                return (bool)b.GetMethod("Verify", new[] { typeof(string), typeof(string) })!.Invoke(null, new object[] { password, stored })!;
            }
            var d = Convert.FromBase64String(stored);
            if (d.Length > 13 && d[0] == 0x01) // ASP.NET Identity v3
            {
                int prf = Rd(d, 1), iter = Rd(d, 5), sl = Rd(d, 9); var salt = d.AsSpan(13, sl).ToArray(); var sub = d.AsSpan(13 + sl).ToArray();
                var alg = prf == 0 ? HashAlgorithmName.SHA1 : prf == 1 ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA512;
                return CryptographicOperations.FixedTimeEquals(Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, alg, sub.Length), sub);
            }
        }
        catch (InvalidOperationException) { throw; } catch { }
        return false;
    }
    static int Rd(byte[] b, int o) => (b[o] << 24) | (b[o + 1] << 16) | (b[o + 2] << 8) | b[o + 3];
    static void Wr(byte[] b, int o, int v) { b[o] = (byte)(v >> 24); b[o + 1] = (byte)(v >> 16); b[o + 2] = (byte)(v >> 8); b[o + 3] = (byte)v; }
    public static string Kind(string? sample) => sample == null ? "نامشخص (کاربری با رمز نیست)" : sample.StartsWith("$2") ? "BCrypt" : IsV3(sample) ? "ASP.NET Identity v3" : "قالب ناشناخته";
    static bool IsV3(string s) { try { var d = Convert.FromBase64String(s); return d.Length > 13 && d[0] == 1; } catch { return false; } }
    public static string Hash(string password, string? sample)
    {
        if (sample != null && sample.StartsWith("$2"))
        {
            var b = Bcrypt() ?? throw new InvalidOperationException("رمزهای فعلی BCrypt هستند ولی پکیج BCrypt.Net-Next در پروژه نیست. اجرا کنید: dotnet add src/Ghestine.API package BCrypt.Net-Next");
            return (string)b.GetMethod("HashPassword", new[] { typeof(string) })!.Invoke(null, new object[] { password })!;
        }
        if (sample == null || IsV3(sample))
        {
            var salt = RandomNumberGenerator.GetBytes(16); const int iter = 100000;
            var sub = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA512, 32);
            var o = new byte[13 + salt.Length + sub.Length]; o[0] = 1; Wr(o, 1, 2); Wr(o, 5, iter); Wr(o, 9, salt.Length);
            Buffer.BlockCopy(salt, 0, o, 13, salt.Length); Buffer.BlockCopy(sub, 0, o, 13 + salt.Length, sub.Length); return Convert.ToBase64String(o);
        }
        throw new InvalidOperationException("قالب هش رمزهای فعلی ناشناخته است؛ برای جلوگیری از ساخت حساب ناسازگار، ساخت متوقف شد");
    }
}

public class ReflectiveTokenIssuer : IAuthTokenIssuer
{
    private readonly AuthAdapter _a; public ReflectiveTokenIssuer(AuthAdapter a) => _a = a;
    public async Task<object?> IssueForPhoneAsync(string phone, CancellationToken ct)
    { var u = await _a.FindByPhoneAsync(phone, ct); return u == null || !_a.IsActive(u) ? null : _a.IssueToken(u); }
}

// ---------- ابزارهای خط فرمان: --auth-doctor و --create-superadmin ----------
public static class AuthTools
{
    static string ReadHidden(string prompt)
    {
        Console.Write(prompt); var sb = new StringBuilder();
        while (true) { var k = Console.ReadKey(true); if (k.Key == ConsoleKey.Enter) { Console.WriteLine(); break; } if (k.Key == ConsoleKey.Backspace) { if (sb.Length > 0) sb.Length--; } else if (!char.IsControl(k.KeyChar)) sb.Append(k.KeyChar); }
        return sb.ToString();
    }
    public static async Task RunAsync(IServiceProvider sp, string[] args)
    {
        using var scope = sp.CreateScope(); var a = scope.ServiceProvider.GetRequiredService<AuthAdapter>(); var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var m = a.Map();
        Console.WriteLine("=== تشخیص احراز هویت (بدون چاپ رمز یا کلید) ===");
        if (m == null) { Console.WriteLine("موجودیت کاربر: پیدا نشد. دلیل: " + a.WhyNot); Environment.ExitCode = 2; return; }
        string Pn(PropertyInfo? p) => p?.Name ?? "—";
        Console.WriteLine($"موجودیت: {m.Clr.Name} | Id={Pn(m.Id)} Phone={Pn(m.Phone)} Hash={Pn(m.Hash)} Name={Pn(m.Name)} Role={Pn(m.Role)} Active={Pn(m.Active)} Email={Pn(m.Email)} NationalCode={Pn(m.Nid)} Tenant={Pn(m.Tenant)}");
        foreach (var n in m.Notes) Console.WriteLine("هشدار: " + n);
        var sample = await a.SampleHashAsync(default);
        Console.WriteLine("قالب هش رمز: " + PasswordHelper.Kind(sample));
        Console.WriteLine("کلید JWT: " + (new[] { "Jwt:Key", "Jwt:Secret", "JwtSettings:Key", "JwtSettings:Secret", "Authentication:Jwt:Key", "Jwt:SecretKey" }.Any(k => !string.IsNullOrWhiteSpace(cfg[k])) ? "تنظیم شده" : "پیدا نشد"));
        Console.WriteLine("کاوه‌نگار: " + (string.IsNullOrWhiteSpace(cfg["KAVENEGAR_API_KEY"]) || cfg["KAVENEGAR_API_KEY"] == "YOUR_API_KEY" ? "کلید تنظیم نشده" : "کلید تنظیم شده") + " | فرستنده: " + (cfg["KAVENEGAR_SENDER"] ?? "—"));
        if (!args.Contains("--create-superadmin")) return;

        Console.WriteLine("\n=== ساخت سوپرادمین (اطلاعات را همین‌جا وارد کنید؛ هیچ‌چیز در کد یا لاگ ذخیره نمی‌شود) ===");
        if (m.Role == null || m.Id == null) { Console.WriteLine("ساخت متوقف شد: ستون نقش یا Id پیدا نشد."); Environment.ExitCode = 2; return; }
        Console.Write("نام و نام خانوادگی [بهروز حسین‌زاده]: "); var name = Console.ReadLine(); if (string.IsNullOrWhiteSpace(name)) name = "بهروز حسین‌زاده";
        Console.Write("شماره موبایل (09...): "); var phone = OtpService.NormPhone(Console.ReadLine()); if (phone == "") { Console.WriteLine("شماره معتبر نیست."); Environment.ExitCode = 2; return; }
        Console.Write("ایمیل: "); var email = Console.ReadLine()?.Trim();
        string? nid = null; if (m.Nid != null) { Console.Write("کد ملی (اختیاری؛ فقط برای ثبت در پرونده): "); nid = Console.ReadLine()?.Trim(); if (string.IsNullOrEmpty(nid)) nid = null; }
        var p1 = ReadHidden("رمز اولیه (حداقل ۱۲ نویسه): "); var p2 = ReadHidden("تکرار رمز: ");
        if (p1 != p2 || p1.Length < 12) { Console.WriteLine("رمزها یکسان نیستند یا کمتر از ۱۲ نویسه‌اند."); Environment.ExitCode = 2; return; }
        var (u, err) = await a.CreateUserAsync(name!, phone, email, nid, p1, "SuperAdmin", null, true, default);
        if (u == null) { Console.WriteLine("ساخته نشد: " + err); Environment.ExitCode = 2; return; }
        var back = await a.FindByPhoneAsync(phone, default); var ok = back != null && a.VerifyPassword(back, p1) && a.RoleOf(back) == "SuperAdmin";
        Console.WriteLine(ok ? "حساب در پایگاه داده ساخته شد و بررسی رمز و نقش موفق بود. ورود واقعی را در مرورگر آزمایش کنید؛ تا آن موقع فعال اعلام نکنید."
                              : "حساب ساخته شد ولی بررسی رمز/نقش ناموفق بود. حساب را فعال ندانید و گزارش دهید.");
        Console.WriteLine("رویداد: ساخت SuperAdmin برای شماره ••" + phone[^4..] + " در " + DateTime.UtcNow.ToString("u"));
        if (!ok) Environment.ExitCode = 3;
    }
}

public static class Ticket
{
    public static string Make(string phone, string pepper) { var exp = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(); var p = $"{phone}|{exp}"; return Convert.ToBase64String(Encoding.UTF8.GetBytes(p)) + "." + Sig(p, pepper); }
    public static string? Check(string? t, string pepper)
    {
        try { var parts = (t ?? "").Split('.'); if (parts.Length != 2) return null; var p = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(parts[1]), Encoding.ASCII.GetBytes(Sig(p, pepper)))) return null;
            var s = p.Split('|'); return long.Parse(s[1]) < DateTimeOffset.UtcNow.ToUnixTimeSeconds() ? null : s[0]; } catch { return null; }
    }
    static string Sig(string p, string pepper) => Convert.ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(pepper)).ComputeHash(Encoding.UTF8.GetBytes("ticket:" + p)));
}
