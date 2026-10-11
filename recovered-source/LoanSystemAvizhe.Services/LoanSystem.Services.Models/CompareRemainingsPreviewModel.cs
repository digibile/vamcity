namespace LoanSystem.Services.Models;

public class CompareRemainingsPreviewModel : BaseModel
{
	public string NationalCode { get; set; }

	public string PersonalCode { get; set; }

	public int LoanCode { get; set; }

	public decimal Remain { get; set; }

	public string SystemMsg { get; set; }
}
