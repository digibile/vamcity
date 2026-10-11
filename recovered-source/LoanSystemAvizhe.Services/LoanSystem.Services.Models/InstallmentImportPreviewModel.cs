using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class InstallmentImportPreviewModel : BaseModel
{
	public string NationalCode { get; set; }

	public string FullName { get; set; }

	public int UserLoanCode { get; set; }

	public int InstallmentNumber { get; set; }

	public decimal Installment { get; set; }

	public string PaymentDate { get; set; }

	public PaymentMethods PaymentMethod { get; set; }

	public string SystemMsg { get; set; }
}
