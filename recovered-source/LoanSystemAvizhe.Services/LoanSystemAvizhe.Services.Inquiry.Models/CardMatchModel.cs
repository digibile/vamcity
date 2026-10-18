using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Services.Inquiry.Models;

public class CardMatchModel
{
    public MemberModel Member { get; set; }

    public string CardNumber { get; set; }
}
