using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Models.IranValidity;

public class IranValidityConfig
{
    public bool IsActive { get; set; }

    public string BaseUrl { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public int Price { get; set; }

    public OnlinePaymentGateways PaymentGateway { get; set; }
}
