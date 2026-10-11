using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class LoanContactModel : BaseModel
{
	[Required(ErrorMessage = "طرف حساب الزامی است.")]
	public ContactModel Contact { get; set; }

	[Required(ErrorMessage = "وام الزامی است.")]
	public Loan2Model Loan { get; set; }

	[Required(ErrorMessage = "سهم طرف حساب الزامی است")]
	[Range(0.0, double.MaxValue, ErrorMessage = "سهم نماینده نمی تواند کمتر از 0 باشد")]
	public decimal Share { get; set; }

	public CustomerModel Customer { get; set; }

	public LoanContactModel()
	{
	}

	public LoanContactModel(LoanContact loanContact)
	{
		Id = loanContact.Id;
		Share = loanContact.Share;
		if (loanContact.Contact != null)
		{
			Contact = new ContactModel(loanContact.Contact);
		}
		if (loanContact.Loan != null)
		{
			Loan = new Loan2Model
			{
				Id = loanContact.LoanId,
				Title = loanContact.Loan.Title
			};
		}
		if (loanContact.Customer != null)
		{
			Customer = new CustomerModel(loanContact.Customer);
		}
	}
}
