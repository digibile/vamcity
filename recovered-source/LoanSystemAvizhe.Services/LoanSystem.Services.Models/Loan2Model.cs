using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class Loan2Model : BaseModel
{
	public string Title { get; set; }

	public LoanProfitType LoanProfitType { get; set; }

	public double ProfitPercent { get; set; }

	public double EarlyPercent { get; set; }

	public double DelayPercent { get; set; }

	public bool IsSponsored { get; set; }

	public string AllowedInstallments { get; set; }

	public bool Disable { get; set; }

	public string DisableText { get; set; }

	public bool AssignedToSponsor { get; set; }
}
