using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanRequestSponsorHistoryModel : BaseModel
{
	public User2Model User { get; set; }

	[Required(ErrorMessage = "شناسه درخواست وام الزامی است.")]
	public Guid UserLoanRequestId { get; set; }

	public DateTime DateTime { get; set; }

	public PersianDate DateTime_Persian => DateTime.ToPersianDateObject();

	public string Subject { get; }

	public string Description { get; }

	public Guid CustomerId { get; set; }

	public UserLoanRequestSponsorHistoryModel()
	{
	}

	public UserLoanRequestSponsorHistoryModel(UserLoanRequestSponsorHistory userLoanRequestSponsorHistory)
	{
		Id = userLoanRequestSponsorHistory.Id;
		UserLoanRequestId = userLoanRequestSponsorHistory.UserLoanRequestId;
		DateTime = userLoanRequestSponsorHistory.DateTime;
		Subject = userLoanRequestSponsorHistory.Subject;
		Description = userLoanRequestSponsorHistory.Description;
		if (userLoanRequestSponsorHistory.User != null)
		{
			User = new User2Model(userLoanRequestSponsorHistory.User);
		}
		CustomerId = userLoanRequestSponsorHistory.CustomerId;
	}
}
