using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace Ghestine.API.Core;

/// OCR با Tesseract روی خود سرور (بدون ارسال عکس به سرویس خارجی). نصب: apt install tesseract-ocr tesseract-ocr-fas
/// اگر Tesseract نصب نباشد، متن خالی برمی‌گردد و مدرک «در انتظار بررسی دستی» می‌ماند.
public class TesseractOcrProvider : IOcrProvider
{
    private static readonly SemaphoreSlim Gate = new(2, 2);   // حداکثر دو OCR هم‌زمان
    private readonly ILogger<TesseractOcrProvider> _log; private readonly IConfiguration _cfg;
    public TesseractOcrProvider(ILogger<TesseractOcrProvider> log, IConfiguration cfg) { _log = log; _cfg = cfg; }

    public async Task<(string? Text, string? FieldsJson)> ExtractAsync(byte[] image, string contentType, string kind, CancellationToken ct)
    {
        var bin = _cfg["Ocr:TesseractPath"] ?? "tesseract"; var langs = _cfg["Ocr:Languages"] ?? "fas+eng";
        if (!Regex.IsMatch(langs, "^[a-z_+]{2,30}$")) langs = "fas+eng";
        var ext = contentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
        var tmp = Path.Combine(Path.GetTempPath(), "ocr-" + Guid.NewGuid().ToString("N") + ext);
        await Gate.WaitAsync(ct);
        try
        {
            await File.WriteAllBytesAsync(tmp, image, ct);
            var psi = new ProcessStartInfo(bin) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            foreach (var a in new[] { tmp, "stdout", "-l", langs, "--psm", "6" }) psi.ArgumentList.Add(a);   // آرگومان‌ها جدا؛ بدون shell
            using var p = Process.Start(psi)!;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct); cts.CancelAfter(TimeSpan.FromSeconds(40));
            var outT = p.StandardOutput.ReadToEndAsync(cts.Token); var errT = p.StandardError.ReadToEndAsync(cts.Token);
            try { await p.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException) { try { p.Kill(true); } catch { } _log.LogWarning("OCR timeout"); return (null, null); }
            var text = (await outT).Trim(); _ = await errT;
            if (p.ExitCode != 0 || text.Length < 5) { _log.LogInformation("OCR no text, exit {Code}", p.ExitCode); return (null, null); }
            return (text, IdCardParser.Parse(text, kind));
        }
        catch (System.ComponentModel.Win32Exception) { _log.LogWarning("tesseract نصب نیست؛ OCR غیرفعال است"); return (null, null); }
        finally { try { File.Delete(tmp); } catch { } Gate.Release(); }
    }
}

/// استخراج فیلدهای کارت ملی/شناسنامه از متن OCR. فقط کد ملی با کنترل رقم آخر «تأییدشده» است؛ بقیه حدسی‌اند و باید توسط کاربر بازبینی شوند.
public static class IdCardParser
{
    static string Norm(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var c in s)
            sb.Append(c switch { >= '۰' and <= '۹' => (char)('0' + c - '۰'), >= '٠' and <= '٩' => (char)('0' + c - '٠'), 'ي' => 'ی', 'ك' => 'ک', '‌' => ' ', _ => c });
        return sb.ToString();
    }
    public static string? Parse(string raw, string kind)
    {
        var t = Norm(raw); var f = new Dictionary<string, object?>(); var validated = new List<string>();
        var ids = Regex.Matches(t, @"(?<!\d)\d{10}(?!\d)").Select(m => m.Value).Where(Iran.NationalCodeOk).Distinct().ToList();
        if (ids.Count > 0) { f["nationalCode"] = ids[0]; validated.Add("nationalCode"); }
        var d = Regex.Match(t, @"(?<!\d)(1[34]\d{2})\s*[/\-.]\s*(\d{1,2})\s*[/\-.]\s*(\d{1,2})(?!\d)");
        if (d.Success && int.Parse(d.Groups[2].Value) is >= 1 and <= 12 && int.Parse(d.Groups[3].Value) is >= 1 and <= 31)
            f["birthDate"] = $"{d.Groups[1].Value}/{int.Parse(d.Groups[2].Value):00}/{int.Parse(d.Groups[3].Value):00}";
        string? Grab(string key) { var m = Regex.Match(t, key + @"\s*[:：\-]?\s*([؀-ۿ]{2,}(?: [؀-ۿ]{2,})?)"); return m.Success ? m.Groups[1].Value.Trim() : null; }
        f["lastName"] = Grab("نام خانوادگی");
        f["fatherName"] = Grab("نام پدر");
        f["firstName"] = Grab(@"نام(?!\s*(?:پدر|خانوادگی))");
        var clean = f.Where(k => k.Value != null).ToDictionary(k => k.Key, k => k.Value);
        if (clean.Count == 0) return null;
        return JsonSerializer.Serialize(new { fields = clean, validated, note = "فقط کد ملی با کنترل رقم آخر بررسی شده؛ بقیه‌ی فیلدها را خودتان بازبینی کنید." });
    }
}
