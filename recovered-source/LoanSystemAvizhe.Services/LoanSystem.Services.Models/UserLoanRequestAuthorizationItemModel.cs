using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class UserLoanRequestAuthorizationItemModel : BaseModel
{
	public int Code { get; set; }

	public List<IFormFile> Files { get; set; }

	public UserLoanRequestModel UserLoanRequest { get; set; }

	public AuthorizationItemModel AuthorizationItem { get; set; }

	public ThirdPersonAssuranceAuthorizationItemModel ThirdPersonAssuranceAuthorizationItem { get; set; }

	public EmzameContractAuthorizationItemModel EmzameContractAuthorizationItem { get; set; }

	public DateTime CreateDate { get; set; }

	public PersianDate CreateDate_Persian => CreateDate.ToPersianDateObject();

	public DateTime LastChangeDate { get; set; }

	public PersianDate LastChangeDate_Persian => LastChangeDate.ToPersianDateObject();

	public string Attachment { get; set; }

	public bool AttachmentByAdmin { get; set; }

	[StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
	public string MemberDescription { get; set; } = "";

	[StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
	public string AdminDescription { get; set; } = "";

	public string Tag { get; set; }

	public LoanRequestAuthorizationStatus Status { get; set; }

	public string Status_Text => Status.ToString().Replace("_", " ");

	public Guid CustomerId { get; set; }

	public List<UserLoanRequestAuthorizationItemAttachmentModel> Attachments { get; set; } = new List<UserLoanRequestAuthorizationItemAttachmentModel>();

	public UserLoanRequestAuthorizationItemModel()
	{
	}

	public UserLoanRequestAuthorizationItemModel(UserLoanRequestAuthorizationItem item)
	{
		Id = item.Id;
		Code = item.Code;
		if (item.UserLoanRequest != null)
		{
			UserLoanRequest = new UserLoanRequestModel(item.UserLoanRequest);
		}
		if (item.AuthorizationItem != null)
		{
			AuthorizationItem = new AuthorizationItemModel(item.AuthorizationItem);
		}
		if (item.ThirdPersonAssuranceAuthorizationItem != null)
		{
			ThirdPersonAssuranceAuthorizationItem = new ThirdPersonAssuranceAuthorizationItemModel(item.ThirdPersonAssuranceAuthorizationItem);
		}
		if (item.EmzameContractAuthorizationItem != null)
		{
			EmzameContractAuthorizationItem = new EmzameContractAuthorizationItemModel(item.EmzameContractAuthorizationItem);
		}
		CreateDate = item.CreateDate;
		LastChangeDate = item.LastChangeDate;
		Attachment = item.Attachment;
		AttachmentByAdmin = item.AttachmentByAdmin;
		MemberDescription = item.MemberDescription ?? "";
		AdminDescription = item.AdminDescription ?? "";
		Tag = item.Tag;
		Status = item.Status;
		CustomerId = item.CustomerId;
		if (item.Attachments == null)
		{
			return;
		}
		foreach (UserLoanRequestAuthorizationItemAttachment attachment in item.Attachments)
		{
			Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel(attachment));
		}
	}
}
