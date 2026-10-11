using System;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class MemberAuthorizationItemModelLight : BaseModel
{
	public long MemberId { get; set; }

	public string MemberFullName { get; set; }

	public string MemberNationalCode { get; set; }

	public Guid AuthorizationItemId { get; set; }

	public string AuthorizationItemTitle { get; set; }

	public DateTime CreateDate { get; set; }

	public PersianDate CreateDate_Persian => CreateDate.ToPersianDateObject();

	public DateTime LastChangeDate { get; set; }

	public PersianDate LastChangeDate_Persian => LastChangeDate.ToPersianDateObject();

	public string LastChangeBy { get; set; }

	public LoanRequestAuthorizationStatus Status { get; set; }

	public string Status_Text => Status.ToString().Replace("_", " ");

	public MemberAuthorizationItemModelLight()
	{
	}

	public MemberAuthorizationItemModelLight(MemberAuthorizationItem item)
	{
		Id = item.Id;
		MemberFullName = item?.Member?.FullName;
		MemberId = (item?.Member?.Id).GetValueOrDefault();
		MemberNationalCode = item.Member?.NationalCode;
		AuthorizationItemId = item?.AuthorizationItemId ?? Guid.Empty;
		AuthorizationItemTitle = item?.AuthorizationItem?.Title;
		CreateDate = item.CreateDate;
		LastChangeDate = item.LastChangeDate;
		LastChangeBy = item.LastChangeBy;
		Status = item.Status;
	}
}
