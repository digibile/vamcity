using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class MemberAuthorizationItemModel : BaseModel
{
	public List<IFormFile> Files { get; set; }

	public Member4Model Member { get; set; }

	public AuthorizationItemModel AuthorizationItem { get; set; }

	public DateTime CreateDate { get; set; }

	public PersianDate CreateDate_Persian => CreateDate.ToPersianDateObject();

	public DateTime LastChangeDate { get; set; }

	public string LastChangeBy { get; set; }

	public PersianDate LastChangeDate_Persian => LastChangeDate.ToPersianDateObject();

	[StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
	public string MemberDescription { get; set; } = "";

	[StringLength(1000, MinimumLength = 0, ErrorMessage = "طول کاراکترهای توضیحات نباید بیشتر از 1000 باشد")]
	public string AdminDescription { get; set; } = "";

	public string Tag { get; set; }

	public LoanRequestAuthorizationStatus Status { get; set; }

	public string Status_Text => Status.ToString().Replace("_", " ");

	public Guid CustomerId { get; set; }

	public List<MemberAuthorizationItemAttachmentModel> Attachments { get; set; } = new List<MemberAuthorizationItemAttachmentModel>();

	public MemberAuthorizationItemModel()
	{
	}

	public MemberAuthorizationItemModel(MemberAuthorizationItem item)
	{
		Id = item.Id;
		if (item.Member != null)
		{
			Member = new Member4Model(item.Member);
		}
		if (item.AuthorizationItem != null)
		{
			AuthorizationItem = new AuthorizationItemModel(item.AuthorizationItem);
		}
		CreateDate = item.CreateDate;
		LastChangeDate = item.LastChangeDate;
		LastChangeBy = item.LastChangeBy;
		MemberDescription = item.MemberDescription ?? "";
		AdminDescription = item.AdminDescription ?? "";
		Tag = item.Tag;
		Status = item.Status;
		CustomerId = item.CustomerId;
		if (item.Attachments == null)
		{
			return;
		}
		foreach (MemberAuthorizationItemAttachment attachment in item.Attachments)
		{
			Attachments.Add(new MemberAuthorizationItemAttachmentModel(attachment));
		}
	}
}
