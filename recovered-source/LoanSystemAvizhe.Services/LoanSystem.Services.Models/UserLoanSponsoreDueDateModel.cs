using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanSponsoreDueDateModel : BaseModel
{
	[Required(ErrorMessage = "وام الزامی است.")]
	public Guid UserLoanId { get; set; }

	[Required(ErrorMessage = "مبلغ الزامی است")]
	[Range(0.0, double.MaxValue, ErrorMessage = "مبلغ نمی تواند کمتر از 0 باشد")]
	public decimal Amount { get; set; }

	public DateTime DueDate { get; set; }

	public bool IsCheckouted { get; set; }

	public PersianDate DueDate_Persian => DueDate.ToPersianDateObject();

	public DateTime UserLoanStartDate { get; set; }

	public PersianDate UserLoanStartDate_Persian => UserLoanStartDate.ToPersianDateObject();

	public int UserLoanCode { get; set; }

	public string UserLoanTitle { get; set; }

	public decimal UserLoanAmount { get; set; }

	public Member2Model MemberModel { get; set; }

	public string Description { get; set; }

	public Guid CustomerId { get; set; }

	public UserLoanSponsoreDueDateModel()
	{
	}

	public UserLoanSponsoreDueDateModel(UserLoanSponsoreDueDate userLoanSponsoreDueDate)
	{
		Id = userLoanSponsoreDueDate.Id;
		Amount = userLoanSponsoreDueDate.Amount;
		DueDate = userLoanSponsoreDueDate.DueDate;
		CustomerId = userLoanSponsoreDueDate.CustomerId;
		UserLoanId = userLoanSponsoreDueDate.UserLoanId;
		IsCheckouted = userLoanSponsoreDueDate.IsCheckouted;
		if (userLoanSponsoreDueDate.UserLoan != null)
		{
			UserLoanCode = userLoanSponsoreDueDate.UserLoan.Code;
			UserLoanAmount = userLoanSponsoreDueDate.UserLoan.Amount;
			UserLoanStartDate = userLoanSponsoreDueDate.UserLoan.StartDate;
			UserLoanTitle = userLoanSponsoreDueDate.UserLoan.Loan?.Title ?? "";
			if (userLoanSponsoreDueDate.UserLoan.User != null)
			{
				MemberModel = new Member2Model(userLoanSponsoreDueDate.UserLoan.User);
			}
		}
		Description = userLoanSponsoreDueDate.Description;
	}
}
