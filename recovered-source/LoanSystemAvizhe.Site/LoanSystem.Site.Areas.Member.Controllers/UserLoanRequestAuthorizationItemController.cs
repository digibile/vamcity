using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Agent,Admin")]
public class UserLoanRequestAuthorizationItemController : BaseMemberController
{
	private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

	private readonly IUserLoanRequestAuthorizationItemAttachmentService _userLoanRequestAuthorizationItemAttachmentService;

	private readonly IFileHelper _fileHelper;

	private readonly IntegratedSmsSenderService _integratedSmsSenderService;

	private readonly ICurrentUserService _currentUserService;

	public UserLoanRequestAuthorizationItemController(ILogger<UserLoanRequestAuthorizationItemController> logger, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IUserLoanRequestAuthorizationItemAttachmentService userLoanRequestAuthorizationItemAttachmentService, ICurrentUserService currentUserService, IFileHelper fileHelper, IntegratedSmsSenderService integratedSmsSenderService)
		: base(logger)
	{
		_userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
		_userLoanRequestAuthorizationItemAttachmentService = userLoanRequestAuthorizationItemAttachmentService;
		_currentUserService = currentUserService;
		_fileHelper = fileHelper;
		_integratedSmsSenderService = integratedSmsSenderService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<UserLoanRequestAuthorizationItemModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItemModel model = new UserLoanRequestAuthorizationItemModel(await _userLoanRequestAuthorizationItemService.GetById(id, "ThirdPersonAssuranceAuthorizationItem.User", "EmzameContractAuthorizationItem", "AuthorizationItem", "Attachments"));
			if (model.Attachments?.Any() ?? false)
			{
				model.Attachments.ForEach((UserLoanRequestAuthorizationItemAttachmentModel a) =>
				{
					a.FillFileProps(_fileHelper, model.CustomerId);
				});
			}
			else
			{
				model.Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel
				{
					RowState = "new"
				});
			}
			return Success(model);
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<UserLoanRequestAuthorizationItemModel>>> ListByUserLoanRequestId(Guid id)
	{
		return TryCatch(async () =>
		{
			List<UserLoanRequestAuthorizationItem> entities = await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.UserLoanRequestId == id, "AuthorizationItem", "ThirdPersonAssuranceAuthorizationItem.User", "EmzameContractAuthorizationItem", "Attachments");
			List<UserLoanRequestAuthorizationItemModel> models = entities.Select((UserLoanRequestAuthorizationItem x) => new UserLoanRequestAuthorizationItemModel(x)).ToList();
			foreach (UserLoanRequestAuthorizationItemModel model in models)
			{
				if (!string.IsNullOrEmpty(model.Attachment))
				{
					UserLoanRequestAuthorizationItemAttachment attachment = new UserLoanRequestAuthorizationItemAttachment
					{
						Attachment = model.Attachment,
						AttachmentByAdmin = model.AttachmentByAdmin,
						CustomerId = model.CustomerId,
						MemberDescription = model.MemberDescription,
						UserLoanRequestAuthorizationItemId = model.Id
					};
					await _userLoanRequestAuthorizationItemAttachmentService.Insert(attachment);
					model.Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel(attachment));
					UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entities.FirstOrDefault((UserLoanRequestAuthorizationItem ent) => ent.Id == model.Id);
					userLoanRequestAuthorizationItem.Attachment = "";
					userLoanRequestAuthorizationItem.MemberDescription = "";
					await _userLoanRequestAuthorizationItemService.Update(userLoanRequestAuthorizationItem);
				}
				if (model.Attachments?.Any() ?? false)
				{
					model.Attachments.ForEach((UserLoanRequestAuthorizationItemAttachmentModel a) =>
					{
						a.FillFileProps(_fileHelper, model.CustomerId);
					});
				}
				else
				{
					model.Attachments.Add(new UserLoanRequestAuthorizationItemAttachmentModel
					{
						RowState = "new"
					});
				}
			}
			return Success(models);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<UserLoanRequestAuthorizationItemModel>> Edit(UserLoanRequestAuthorizationItemModel model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.Id, "Attachments")).FirstOrDefault();
			if (entity == null)
			{
				return Error<UserLoanRequestAuthorizationItemModel>(null, "آیتم مدرک یافت نشد.");
			}
			if (entity.Status == LoanRequestAuthorizationStatus.در_انتطار_بررسی_کارشناس || entity.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
			{
				return Error<UserLoanRequestAuthorizationItemModel>(null, "وضعیت آیتم اجازه ویرایش ندارد.");
			}
			string text = ValidateAttachments(model.Attachments);
			if (!string.IsNullOrEmpty(text))
			{
				return Error<UserLoanRequestAuthorizationItemModel>(null, text);
			}
			await PrepareAttachments(model.Attachments, entity);
			entity.Attachment = model.Attachment;
			entity.MemberDescription = new HtmlSanitizer().Sanitize(model.MemberDescription);
			entity.Status = LoanRequestAuthorizationStatus.ثبت_شده_توسط_کاربر;
			entity.LastChangeDate = DateTime.Now;
			await _userLoanRequestAuthorizationItemService.Update(entity);
			UserLoanRequestAuthorizationItemModel data = new UserLoanRequestAuthorizationItemModel
			{
				Id = entity.Id,
				Status = entity.Status
			};
			return Success(data, "عملیات با موفقیت انجام شد.");
		});
	}

	private string ValidateAttachments(List<UserLoanRequestAuthorizationItemAttachmentModel> attachments)
	{
		int num = 1;
		foreach (UserLoanRequestAuthorizationItemAttachmentModel attachment in attachments)
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

	private async Task PrepareAttachments(List<UserLoanRequestAuthorizationItemAttachmentModel> attachments, UserLoanRequestAuthorizationItem entity)
	{
		foreach (UserLoanRequestAuthorizationItemAttachmentModel attachment in attachments)
		{
			if (attachment.RowState == "new" && string.IsNullOrEmpty(attachment.MemberDescription) && attachment.File == null)
			{
				continue;
			}
			if (attachment.File != null)
			{
				UserLoanRequestAuthorizationItemAttachmentModel userLoanRequestAuthorizationItemAttachmentModel = attachment;
				userLoanRequestAuthorizationItemAttachmentModel.Attachment = await _fileHelper.UploadFile(attachment.File, FileUploadDirectory.AuthorizeItem, _currentUserService.CustomerId.ToString());
			}
			if (attachment.Id != Guid.Empty)
			{
				bool flag = false;
				UserLoanRequestAuthorizationItemAttachment att = entity.Attachments.FirstOrDefault((UserLoanRequestAuthorizationItemAttachment x) => x.Id == attachment.Id);
				if (attachment.RowState == "deleted")
				{
					if (att != null)
					{
						await _userLoanRequestAuthorizationItemAttachmentService.Delete(att);
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
					await _userLoanRequestAuthorizationItemAttachmentService.Update(att);
				}
			}
			else if (attachment.RowState == "new" || attachment.Id == Guid.Empty)
			{
				await _userLoanRequestAuthorizationItemAttachmentService.Insert(new UserLoanRequestAuthorizationItemAttachment
				{
					Attachment = attachment.Attachment,
					MemberDescription = attachment.MemberDescription,
					AttachmentByAdmin = attachment.AttachmentByAdmin,
					CustomerId = entity.CustomerId,
					UserLoanRequestAuthorizationItemId = entity.Id
				});
			}
		}
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == id)).FirstOrDefault();
			if (userLoanRequestAuthorizationItem == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _userLoanRequestAuthorizationItemService.Delete(userLoanRequestAuthorizationItem);
			return Success();
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> SignContractWithOtp([FromBody] SignContractWithOtp model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.Id && x.UserLoanRequest.CustomerId == _currentUserService.CustomerId, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم قرارداد یافت نشد.");
			}
			string text = await _userLoanRequestAuthorizationItemService.ValidateOtpAndSignContract(entity, model.Otp);
			if (!string.IsNullOrWhiteSpace(text))
			{
				return Error(text);
			}
			try
			{
				await _integratedSmsSenderService.SendDigitalSignComplete(new UserLoanRequestAuthorizationItemModel(entity));
			}
			catch (Exception)
			{
			}
			return Success("عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<DefaultResponseState> VerifyOtp([FromBody] VerifyMemberOtp model)
	{
		return TryCatch(async () =>
		{
			UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.UserLoanRequestAuthorizationItemId && x.CustomerId == _currentUserService.CustomerId)).FirstOrDefault();
			if (userLoanRequestAuthorizationItem == null)
			{
				return Error("آیتم مدرک یافت نشد.");
			}
			string text = await _userLoanRequestAuthorizationItemService.ValidateOtpAndVerify(userLoanRequestAuthorizationItem, model.MemberId, model.Otp);
			return (!string.IsNullOrWhiteSpace(text)) ? Error(text) : Success("عملیات با موفقیت انجام شد.");
		});
	}
}
