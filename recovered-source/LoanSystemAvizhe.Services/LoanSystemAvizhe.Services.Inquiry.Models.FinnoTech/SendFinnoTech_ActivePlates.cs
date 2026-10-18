using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTech_ActivePlates : BaseSendInquiry
{
    public string NationalCode { get; set; } = "";

    public string Mobile { get; set; } = "";
}
