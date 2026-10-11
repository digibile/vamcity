namespace LoanSystem.Services.Models;

public class AppSettingsForAgentPanelModel : BaseModel
{
	public decimal IranValidityPrice { get; set; }

	public int CalculatorType { get; set; }

	public decimal AgentFactorCommission { get; set; }

	public decimal MaxAgentFactorCommission { get; set; }

	public bool InquiryPersonInfoInRegister { get; set; }

	public bool InquiryPostalAddressInRegister { get; set; }

	public bool InquiryBankInRegister { get; set; }

	public bool HasRegisterOtpSms { get; set; }
}
