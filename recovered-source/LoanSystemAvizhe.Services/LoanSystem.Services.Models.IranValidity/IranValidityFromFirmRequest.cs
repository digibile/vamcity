namespace LoanSystem.Services.Models.IranValidity;

public class IranValidityFromFirmRequest
{
	public string RealPersonNationalCode { get; set; }

	public string MobileNumber { get; set; }

	public string LegalPersonNationalCode { get; set; } = "";
}
