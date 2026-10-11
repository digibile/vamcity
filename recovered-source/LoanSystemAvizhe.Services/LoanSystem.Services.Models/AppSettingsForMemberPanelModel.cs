using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class AppSettingsForMemberPanelModel : BaseModel
{
	public bool IsOnlinePaymentActive { get; set; }

	public string BankInfo { get; set; }

	public decimal IcePrice { get; set; }

	public decimal IranValidityPrice { get; set; }

	public OnlinePaymentGateways Ice_OnlinePaymentGateway { get; set; }

	public OnlinePaymentGateways IranValidity_OnlinePaymentGateway { get; set; }

	public bool ShowSubmitFirstLoanRequestNotifInMemberPanel { get; set; }

	public int CalculatorType { get; set; }
}
