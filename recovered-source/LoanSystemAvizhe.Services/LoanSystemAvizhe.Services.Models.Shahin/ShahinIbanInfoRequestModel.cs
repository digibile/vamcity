using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinIbanInfoRequestModel : BaseSendInquiry
{
    public string bank { get; set; }

    public string nationalCode { get; set; }

    [Required(ErrorMessage = "شماره حساب مبدا الزامی است.")]
    public string sourceAccount { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
