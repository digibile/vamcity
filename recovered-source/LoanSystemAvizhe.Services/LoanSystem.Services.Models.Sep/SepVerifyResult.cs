namespace LoanSystem.Services.Models.Sep;

public class SepVerifyResult
{
	public SepVerifyResultTransactionDetail TransactionDetail { get; set; }

	public int ResultCode { get; set; }

	public string ResultDescription { get; set; }

	public bool Success { get; set; }
}
