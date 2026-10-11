using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Inquiry.Models;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin,Agent")]
public class MemberAuthorizationItemController : BaseMemberController
{
	private readonly IMemberAuthorizationItemService _memberAuthorizationItemService;

	private readonly IMemberAuthorizationItemAttachmentService _memberAuthorizationItemAttachmentService;

	private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

	private readonly IUserLoanRequestService _userLoanRequestService;

	private readonly ILoanRequestStatusService _loanRequestStatusService;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly UserManager _userManager;

	private readonly IFileHelper _fileHelper;

	private readonly ICurrentUserService _currentUserService;

	public MemberAuthorizationItemController(ILogger<MemberAuthorizationItemController> logger, IMemberAuthorizationItemService userLoanRequestAuthorizationItemService, IMemberAuthorizationItemAttachmentService memberAuthorizationItemAttachmentService, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService, ICurrentUserService currentUserService, IFileHelper fileHelper, IUserLoanRequestService userLoanRequestService, ILoanRequestStatusService loanRequestStatusService, IntegratedSmsSenderService integratedSmsSenderService, UserManager userManager)
		: base(logger)
	{
		_memberAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_memberAuthorizationItemAttachmentService = memberAuthorizationItemAttachmentService;
		_thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
		_currentUserService = currentUserService;
		_userLoanRequestService = userLoanRequestService;
		_loanRequestStatusService = loanRequestStatusService;
		_integratedSmsSenderService = integratedSmsSenderService;
		_userManager = userManager;
		_fileHelper = fileHelper;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<MemberAuthorizationItemModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			MemberAuthorizationItemModel model = new MemberAuthorizationItemModel(await _memberAuthorizationItemService.GetById(id, "AuthorizationItem", "Attachments"));
			if (model.Attachments?.Any() ?? false)
			{
				model.Attachments.ForEach((MemberAuthorizationItemAttachmentModel a) =>
				{
					a.FillFileProps(_fileHelper, model.CustomerId);
				});
			}
			else
			{
				model.Attachments.Add(new MemberAuthorizationItemAttachmentModel
				{
					RowState = "new"
				});
			}
			return Success(model);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<MemberAuthorizationItemModel>>> SyncAndGetMemberAuthorizationsItems(long id)
	{
		return TryCatch(async () =>
		{
			List<MemberAuthorizationItemModel> list = await _memberAuthorizationItemService.SyncAndGetMemberAuthorizationsItems(id);
			foreach (MemberAuthorizationItemModel model in list)
			{
				if (model.Attachments?.Any() ?? false)
				{
					model.Attachments.ForEach((MemberAuthorizationItemAttachmentModel a) =>
					{
						a.FillFileProps(_fileHelper, model.CustomerId);
					});
				}
				else
				{
					model.Attachments.Add(new MemberAuthorizationItemAttachmentModel
					{
						RowState = "new"
					});
				}
			}
			return Success(list);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberAuthorizationItemModel>> Create(MemberAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			long memberId = (_currentUserService.IsInRole("Member") ? _currentUserService.UserId.Value : model.Member.Id);
			if ((await _memberAuthorizationItemService.GetByCondition((MemberAuthorizationItem x) => x.AuthorizationItemId == model.AuthorizationItem.Id && x.MemberId == memberId)).Any())
			{
				return Error<MemberAuthorizationItemModel>(null, "این آیتم قبلا جز آیتم های احراز هویت شما ایجاد شده است.");
			}
			MemberAuthorizationItem memberAuthorizationItem = new MemberAuthorizationItem
			{
				CreateDate = DateTime.Now
			};
			MemberAuthorizationItem memberAuthorizationItem2 = memberAuthorizationItem;
			memberAuthorizationItem2.LastChangeBy = (await _userManager.GetById(_currentUserService.UserId.Value))?.FullName;
			memberAuthorizationItem.AuthorizationItemId = model.AuthorizationItem.Id;
			memberAuthorizationItem.Status = LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس;
			memberAuthorizationItem.MemberId = memberId;
			memberAuthorizationItem.CustomerId = _currentUserService.CustomerId.Value;
			memberAuthorizationItem.MemberDescription = model.MemberDescription;
			MemberAuthorizationItem newItem = memberAuthorizationItem;
			await _memberAuthorizationItemService.Insert(newItem);
			await PrepareAttachments(model.Attachments, newItem);
			MemberAuthorizationItemModel data = new MemberAuthorizationItemModel
			{
				Id = newItem.Id,
				Status = newItem.Status
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<MemberAuthorizationItemModel>> Edit(MemberAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			MemberAuthorizationItem entity = (await _memberAuthorizationItemService.GetByCondition((MemberAuthorizationItem x) => x.Id == model.Id, "Member", "Attachments", "AuthorizationItem")).FirstOrDefault();
			if (entity == null)
			{
				return Error<MemberAuthorizationItemModel>(null, "آیتم مدرک یافت نشد.");
			}
			if (entity.AuthorizationItem.SystemType == AuthorizationItemSystemType.DigitalSignSms && entity.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر && model.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error<MemberAuthorizationItemModel>(null, "تغییر وضعیت دستی به تایید شده برای امضای دیجیتال مجاز نمی باشد.");
			}
			if (entity.AuthorizationItem.SystemType == AuthorizationItemSystemType.DigitalSignSms && entity.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر && model.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error<MemberAuthorizationItemModel>(null, "امکان تغییر وضعیت قرارداد تایید شده وجود ندارد.");
			}
			string text = ValidateAttachments(model.Attachments);
			if (!string.IsNullOrEmpty(text))
			{
				return Error<MemberAuthorizationItemModel>(null, text);
			}
			await PrepareAttachments(model.Attachments, entity);
			entity.MemberDescription = model.MemberDescription ?? "";
			entity.Status = LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس;
			entity.LastChangeDate = DateTime.Now;
			MemberAuthorizationItem memberAuthorizationItem = entity;
			memberAuthorizationItem.LastChangeBy = (await _userManager.GetById(_currentUserService.UserId.Value))?.FullName;
			await _memberAuthorizationItemService.Update(entity);
			MemberAuthorizationItemModel data = new MemberAuthorizationItemModel
			{
				Id = entity.Id,
				Status = entity.Status
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	private string ValidateAttachments(List<MemberAuthorizationItemAttachmentModel> attachments)
	{
		int num = 1;
		foreach (MemberAuthorizationItemAttachmentModel attachment in attachments)
		{
			if (attachment.File != null)
			{
				if (attachment.File.Length == 0L)
				{
					return $"خطا در ردیف {num}: فایل خالی است.";
				}
				long length = attachment.File.Length;
				if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(attachment.File))
				{
					return $"خطا در ردیف {num}: فقط فایل تصویر و pdf مجاز می باشد.";
				}
			}
		}
		return "";
	}

	private async Task PrepareAttachments(List<MemberAuthorizationItemAttachmentModel> attachments, MemberAuthorizationItem entity)
	{
		foreach (MemberAuthorizationItemAttachmentModel attachment in attachments)
		{
			if (attachment.RowState == "new" && string.IsNullOrEmpty(attachment.MemberDescription) && attachment.File == null)
			{
				continue;
			}
			if (attachment.File != null)
			{
				MemberAuthorizationItemAttachmentModel memberAuthorizationItemAttachmentModel = attachment;
				memberAuthorizationItemAttachmentModel.Attachment = await _fileHelper.UploadFile(attachment.File, FileUploadDirectory.AuthorizeItem, _currentUserService.CustomerId.ToString());
				attachment.AttachmentByAdmin = true;
			}
			if (attachment.Id != Guid.Empty)
			{
				bool flag = false;
				MemberAuthorizationItemAttachment att = entity.Attachments.FirstOrDefault((MemberAuthorizationItemAttachment x) => x.Id == attachment.Id);
				if (attachment.RowState == "deleted")
				{
					if (att != null)
					{
						await _memberAuthorizationItemAttachmentService.Delete(att);
						_fileHelper.DeleteFile(att.Attachment, Directories.AuthorizeItem(_currentUserService.CustomerId.ToString()));
					}
					continue;
				}
				if (attachment.MemberDescription != att.MemberDescription)
				{
					att.MemberDescription = attachment.MemberDescription;
					flag = true;
				}
				if (att != null && att.Attachment != attachment.Attachment)
				{
					_fileHelper.DeleteFile(att.Attachment, Directories.AuthorizeItem(_currentUserService.CustomerId.ToString()));
					att.Attachment = attachment.Attachment;
					flag = true;
				}
				if (flag)
				{
					await _memberAuthorizationItemAttachmentService.Update(att);
				}
			}
			else if (attachment.RowState == "new" || attachment.Id == Guid.Empty)
			{
				await _memberAuthorizationItemAttachmentService.Insert(new MemberAuthorizationItemAttachment
				{
					Attachment = attachment.Attachment,
					MemberDescription = attachment.MemberDescription,
					AttachmentByAdmin = true,
					CustomerId = entity.CustomerId,
					MemberAuthorizationItemId = entity.Id
				});
			}
		}
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			MemberAuthorizationItem memberAuthorizationItem = (await _memberAuthorizationItemService.GetByCondition((MemberAuthorizationItem x) => x.Id == id, "Attachments")).FirstOrDefault();
			if (memberAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			if (memberAuthorizationItem.Attachments?.Any() ?? false)
			{
				foreach (MemberAuthorizationItemAttachment attachment in memberAuthorizationItem.Attachments)
				{
					_fileHelper.DeleteFile(attachment.Attachment, Directories.AuthorizeItem(_currentUserService.CustomerId.ToString()));
				}
			}
			await _memberAuthorizationItemService.Delete(memberAuthorizationItem);
			return Success();
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> VerifyOtp([FromBody] VerifyMemberOtp model)
	{
		return TryCatch(async () =>
		{
			if (_currentUserService.IsInRole("Member") && model.MemberId != _currentUserService.UserId)
			{
				return Error("دسترسی غیرمجاز.");
			}
			MemberAuthorizationItem memberAuthorizationItem = (await _memberAuthorizationItemService.GetByCondition((MemberAuthorizationItem x) => x.Id == model.MemberAuthorizationItemId && x.CustomerId == _currentUserService.CustomerId)).FirstOrDefault();
			if (memberAuthorizationItem == null)
			{
				return Error("آیتم احراز هویت یافت نشد.");
			}
			string text = await _memberAuthorizationItemService.ValidateOtpAndVerify(memberAuthorizationItem, model.MemberId, model.Otp);
			return (!string.IsNullOrWhiteSpace(text)) ? Error(text) : Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpGet("[action]/{memberId}")]
	public Task<ResponseState<bool>> CheckMobileVerified(long memberId)
	{
		return TryCatch(async () =>
		{
			if (_currentUserService.IsInRole("Member") && memberId != _currentUserService.UserId)
			{
				return Error(data: false, "دسترسی غیرمجاز.");
			}
			User user = await _userManager.GetById(memberId);
			return (user == null) ? Error(data: false, "مشتری یافت نشد.") : Success(user.MobileVerified);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<AppInquiryModel>> InquiryRequest([FromBody] InquiryRequestModel inquiryRequestModel)
	{
		return TryCatch(async () =>
		{
			try
			{
				if (_currentUserService.IsInRole("Member") && inquiryRequestModel.MemberId != _currentUserService.UserId)
				{
					return Error<AppInquiryModel>(null, "دسترسی غیرمجاز.");
				}
				if (!inquiryRequestModel.MemberAuthorizationItemId.HasValue)
				{
					return Error<AppInquiryModel>(null, "شناسه گام احراز هویت اجباری است.");
				}
				string text = await _memberAuthorizationItemService.InquiryRequest(inquiryRequestModel);
				if (!string.IsNullOrWhiteSpace(text))
				{
					return Error<AppInquiryModel>(null, text);
				}
				return Success(new AppInquiryModel(), "عملیات با موفقیت انجام شد.");
			}
			catch (CustomException ex)
			{
				return Error<AppInquiryModel>(null, ex.Message);
			}
		});
	}
}
