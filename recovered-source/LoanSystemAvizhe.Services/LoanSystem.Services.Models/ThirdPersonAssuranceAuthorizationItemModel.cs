using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class ThirdPersonAssuranceAuthorizationItemModel : BaseModel
{
	public int Code { get; set; }

	public Guid? UserLoanRequestAuthorizationItemId { get; set; }

	public UserLoanRequestAuthorizationItemModel UserLoanRequestAuthorizationItem { get; set; }

	public Guid? UserLoanId { get; set; }

	public UserLoanModel UserLoan { get; set; }

	[Required(ErrorMessage = "ضامن الزامی است.")]
	public Member3Model User { get; set; }

	public bool IsSigned { get; set; }

	[Required(ErrorMessage = "شناسه مدرک الزامی است.")]
	public Guid AuthorizationItemId { get; set; }

	public string Comment { get; set; }

	public string Tag { get; set; }

	public Guid CustomerId { get; set; }

	public ThirdPersonAssuranceAuthorizationItemModel()
	{
	}

	public ThirdPersonAssuranceAuthorizationItemModel(ThirdPersonAssuranceAuthorizationItem item)
	{
		Id = item.Id;
		Code = item.Code;
		UserLoanRequestAuthorizationItemId = item.UserLoanRequestAuthorizationItemId;
		AuthorizationItemId = item.AuthorizationItemId;
		if (item.User != null)
		{
			User = new Member3Model(item.User);
		}
		IsSigned = item.IsSigned;
		Tag = item.Tag;
		Comment = item.Comment;
		CustomerId = item.CustomerId;
	}

	public void SetUserLoanRequestAuthorizationItem(UserLoanRequestAuthorizationItemModel model)
	{
		UserLoanRequestAuthorizationItem = model;
	}
}
