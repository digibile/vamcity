using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

public class IceConfig
{
    public bool isIceActive { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public int Price { get; set; }

    public OnlinePaymentGateways IcePaymentGateway { get; set; }
}
