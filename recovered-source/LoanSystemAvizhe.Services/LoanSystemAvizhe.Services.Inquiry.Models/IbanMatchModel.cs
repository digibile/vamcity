using LoanSystem.Services.Models;

namespace LoanSystemAvizhe.Services.Inquiry.Models;

public class IbanMatchModel
{
    public MemberModel Member { get; set; }

    public string Iban { get; set; }
}
