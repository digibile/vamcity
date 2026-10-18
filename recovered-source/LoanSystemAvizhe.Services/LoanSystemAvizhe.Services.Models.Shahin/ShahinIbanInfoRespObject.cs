namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinIbanInfoRespObject
{
    public string bank { get; set; }

    public string accountNumber { get; set; }

    public string ibanNumber { get; set; }

    public string firstName { get; set; }

    public string lastName { get; set; }

    public string accountStatus { get; set; }

    public string accountStatusTranslate => accountStatus?.ToLower() switch
    {
        "active" => "فعال",
        "deactive" => "خطا در پاسخ دهی",
        "closed" => "حساب بسته شده",
        "else" => "سایر موارد",
        "blockednondepositability" => "حساب مسدود بدون قابلیت واریز",
        "blockedwithdepositability" => "حساب مسدود با قابلیت واریز",
        "frozen" => "حساب راکد",
        "unknown" => "ناشناخته",
        _ => "نامشخص",
    };

    public string nationalCode { get; set; }
}
