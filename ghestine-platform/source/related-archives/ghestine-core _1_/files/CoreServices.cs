using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
namespace Ghestine.API.Core;

public interface IOcrProvider
{
    /// متن و فیلدهای مدرک را از تصویر بخواند. اگر سرویس OCR وصل نیست، null برگرداند.
    Task<(string? Text, string? FieldsJson)> ExtractAsync(byte[] image, string contentType, string kind, CancellationToken ct);
}
/// تا زمانی که سرویس OCR واقعی (مثلاً Azure/Google/سرویس ایرانی) وصل نشده، متن خالی می‌ماند و مدرک «در انتظار بررسی» می‌شود.
public class NullOcrProvider : IOcrProvider
{
    public Task<(string?, string?)> ExtractAsync(byte[] image, string contentType, string kind, CancellationToken ct) => Task.FromResult<(string?, string?)>((null, null));
}

public interface ICardCrypto
{
    string Protect(string plain);
    string Unprotect(string cipher);
    byte[] ProtectBytes(byte[] b);
    byte[] UnprotectBytes(byte[] b);
    string Hash(string pan);
    string NewPan(string bin);
}
public class CardCrypto : ICardCrypto
{
    private readonly IDataProtector _p, _f;
    private readonly byte[] _pepper;
    public CardCrypto(IDataProtectionProvider dp, IConfiguration cfg)
    {
        _p = dp.CreateProtector("ghestine.cards.v1");
        _f = dp.CreateProtector("ghestine.files.v1");
        _pepper = Encoding.UTF8.GetBytes(cfg["Cards:HashPepper"] ?? "change-me-in-appsettings");
    }
    public string Protect(string plain) => _p.Protect(plain);
    public string Unprotect(string cipher) => _p.Unprotect(cipher);
    public byte[] ProtectBytes(byte[] b) => _f.Protect(b);
    public byte[] UnprotectBytes(byte[] b) => _f.Unprotect(b);
    public string Hash(string pan) => Convert.ToHexString(new HMACSHA256(_pepper).ComputeHash(Encoding.UTF8.GetBytes(pan)));
    public string NewPan(string bin)
    {
        var d = new int[16];
        for (int i = 0; i < bin.Length && i < 6; i++) d[i] = bin[i] - '0';
        for (int i = bin.Length; i < 15; i++) d[i] = RandomNumberGenerator.GetInt32(10);
        int sum = 0;
        for (int i = 0; i < 15; i++) { int x = d[14 - i]; if (i % 2 == 0) { x *= 2; if (x > 9) x -= 9; } sum += x; }
        d[15] = (10 - sum % 10) % 10;
        return string.Concat(d);
    }
}

public static class Iran
{
    public static bool NationalCodeOk(string v)
    {
        if (v is not { Length: 10 } || !v.All(char.IsDigit) || v.Distinct().Count() == 1) return false;
        int s = 0; for (int i = 0; i < 9; i++) s += (v[i] - '0') * (10 - i);
        int r = s % 11, c = v[9] - '0';
        return r < 2 ? c == r : c == 11 - r;
    }
}
