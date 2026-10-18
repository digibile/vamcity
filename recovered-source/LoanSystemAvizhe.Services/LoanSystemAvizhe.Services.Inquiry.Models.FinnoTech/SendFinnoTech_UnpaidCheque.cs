using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_UnpaidCheque : BaseSendInquiry
{
    public string NationalCode { get; set; } = "";
}
