using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIR_Shahkar : BaseSendInquiry
{
    public string NationalCode { get; set; } = "";

    public string Mobile { get; set; } = "";

    public string BirthDate { get; set; } = "";

    public string Iban { get; set; } = "";
}
