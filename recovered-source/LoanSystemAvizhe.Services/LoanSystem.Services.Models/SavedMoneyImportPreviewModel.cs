namespace LoanSystem.Services.Models;

public class SavedMoneyImportPreviewModel : BaseModel
{
	public string NationalCode { get; set; }

	public string PersonalCode { get; set; }

	public string Month { get; set; }

	public string Number { get; set; }

	public decimal Amount { get; set; }

	public decimal TotalSavedMoney { get; set; }

	public string SystemMsg { get; set; }
}
