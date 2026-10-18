using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class UserLoanRequestAuthorizationItemController : BaseAdminController
{
    private readonly IUserLoanRequestAuthorizationItemService _userLoanRequestAuthorizationItemService;

    private readonly IUserLoanRequestAuthorizationItemAttachmentService _userLoanRequestAuthorizationItemAttachmentService;

    private readonly IThirdPersonAssuranceAuthorizationItemService _thirdPersonAssuranceAuthorizationItemService;

    private readonly IUserLoanRequestService _userLoanRequestService;

    private readonly ILoanRequestStatusService _loanRequestStatusService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IFileHelper _fileHelper;

    private readonly ICurrentUserService _currentUserService;

    public UserLoanRequestAuthorizationItemController(ILogger<UserLoanRequestAuthorizationItemController> logger, IUserLoanRequestAuthorizationItemService userLoanRequestAuthorizationItemService, IUserLoanRequestAuthorizationItemAttachmentService userLoanRequestAuthorizationItemAttachmentService, IThirdPersonAssuranceAuthorizationItemService thirdPersonAssuranceAuthorizationItemService, ICurrentUserService currentUserService, IFileHelper fileHelper, IUserLoanRequestService userLoanRequestService, ILoanRequestStatusService loanRequestStatusService, IntegratedSmsSenderService integratedSmsSenderService)
        : base(logger)
    {
        _userLoanRequestAuthorizationItemService = userLoanRequestAuthorizationItemService;
        _userLoanRequestAuthorizationItemAttachmentService = userLoanRequestAuthorizationItemAttachmentService;
        _thirdPersonAssuranceAuthorizationItemService = thirdPersonAssuranceAuthorizationItemService;
        _currentUserService = currentUserService;
        _userLoanRequestService = userLoanRequestService;
        _loanRequestStatusService = loanRequestStatusService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _fileHelper = fileHelper;
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
            List<UserLoanRequestAuthorizationItemModel> list = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.UserLoanRequestId == id, "AuthorizationItem", "ThirdPersonAssuranceAuthorizationItem.User", "EmzameContractAuthorizationItem", "Attachments")).Select((UserLoanRequestAuthorizationItem x) => new UserLoanRequestAuthorizationItemModel(x)).ToList();
            foreach (UserLoanRequestAuthorizationItemModel model in list)
            {
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
            return Success(list);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] CreateUserLoanRequestAuthorizationItemModel model)
    {
        return TryCatch(async () =>
        {
            UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = null;
            if (model.SystemType != AuthorizationItemSystemType.None)
            {
                userLoanRequestAuthorizationItem = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => (int)x.AuthorizationItem.SystemType == (int)model.SystemType && (int)x.Status == 20 && x.UserLoanRequest.UserId == model.MemberId, "UserLoanRequest", "AuthorizationItem")).FirstOrDefault();
            }
            UserLoanRequestAuthorizationItem newItem = new UserLoanRequestAuthorizationItem
            {
                CreateDate = DateTime.Now,
                AuthorizationItemId = model.AuthorizationItemId,
                Status = LoanRequestAuthorizationStatus.در_انتظار_ثبت_توسط_کاربر,
                UserLoanRequestId = model.UserLoanRequestId,
                CustomerId = _currentUserService.CustomerId.Value,
                AdminDescription = ""
            };
            newItem.Attachments.Add(new UserLoanRequestAuthorizationItemAttachment
            {
                Attachment = "",
                AttachmentByAdmin = true,
                CustomerId = _currentUserService.CustomerId.Value,
                MemberDescription = ""
            });
            if (userLoanRequestAuthorizationItem != null)
            {
                newItem.Status = LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر;
                newItem.Attachment = userLoanRequestAuthorizationItem.Attachment;
                newItem.MemberDescription = userLoanRequestAuthorizationItem.MemberDescription;
                newItem.Attachments.Clear();
                foreach (UserLoanRequestAuthorizationItemAttachment attachment in userLoanRequestAuthorizationItem.Attachments)
                {
                    newItem.Attachments.Add(new UserLoanRequestAuthorizationItemAttachment
                    {
                        Attachment = attachment.Attachment,
                        AttachmentByAdmin = attachment.AttachmentByAdmin,
                        CustomerId = attachment.CustomerId,
                        MemberDescription = attachment.MemberDescription
                    });
                }
            }
            await _userLoanRequestAuthorizationItemService.Insert(newItem);
            return Success(newItem.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<UserLoanRequestAuthorizationItemModel>> Edit(UserLoanRequestAuthorizationItemModel model)
    {
        return TryCatch(async () =>
        {
            List<LoanRequestStatus> statuses = await _loanRequestStatusService.GetAllLoanRequestStatuses();
            UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.Id, "UserLoanRequest", "Attachments", "AuthorizationItem")).FirstOrDefault();
            if (entity == null)
            {
                return Error<UserLoanRequestAuthorizationItemModel>(null, "آیتم مدرک یافت نشد.");
            }
            if (entity.AuthorizationItem.SystemType == AuthorizationItemSystemType.DigitalSignSms && entity.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر && model.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
            {
                return Error<UserLoanRequestAuthorizationItemModel>(null, "تغییر وضعیت دستی به تایید شده برای امضای دیجیتال مجاز نمی باشد.");
            }
            if (entity.AuthorizationItem.SystemType == AuthorizationItemSystemType.DigitalSignSms && entity.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر && model.Status != LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
            {
                return Error<UserLoanRequestAuthorizationItemModel>(null, "امکان تغییر وضعیت قرارداد تایید شده وجود ندارد.");
            }
            string text = ValidateAttachments(model.Attachments);
            if (!string.IsNullOrEmpty(text))
            {
                return Error<UserLoanRequestAuthorizationItemModel>(null, text);
            }
            await PrepareAttachments(model.Attachments, entity);
            entity.AdminDescription = model.AdminDescription ?? "";
            entity.Status = model.Status;
            entity.Attachment = model.Attachment;
            await _userLoanRequestAuthorizationItemService.Update(entity);
            if (model.Status == LoanRequestAuthorizationStatus.ناقص && statuses.Any((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.مدارک_ناقص_تکمیل_مدارک))
            {
                entity.UserLoanRequest.StatusId = statuses.First((LoanRequestStatus x) => x.SystemRequestStatus == SystemRequestStatus.مدارک_ناقص_تکمیل_مدارک).Id;
                await _userLoanRequestService.Update(entity.UserLoanRequest);
            }
            UserLoanRequestAuthorizationItemModel data = new UserLoanRequestAuthorizationItemModel
            {
                Id = entity.Id,
                Status = entity.Status
            };
            return Success(data, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> UpdateDigitalSignDescription([FromBody] UpdateDigitalSignDescriptionModel model)
    {
        return TryCatch(async () =>
        {
            UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == model.UserLoanRequestAuthorizationItemId, "UserLoanRequest.User", "UserLoanRequest.Agent", "UserLoanRequest.Loan", "AuthorizationItem")).FirstOrDefault();
            if (entity == null)
            {
                return Error("آیتم مدرک یافت نشد.");
            }
            if (entity.Status == LoanRequestAuthorizationStatus.تایید_شده_توسط_مدیر)
            {
                return Error("آیتم امضا شده است و اجازه تغییر ندارد.");
            }
            UserLoanRequestAuthorizationItemModel userLoanRequestAuthorizationItemModel = new UserLoanRequestAuthorizationItemModel(entity);
            UserLoanRequestAuthorizationItem userLoanRequestAuthorizationItem = entity;
            userLoanRequestAuthorizationItem.MemberDescription = await _userLoanRequestAuthorizationItemService.ReplaceDataByTags(userLoanRequestAuthorizationItemModel, model.ManualFields, entity.AuthorizationItem.Description);
            await _userLoanRequestAuthorizationItemService.Update(entity);
            return Success("عملیات با موفقیت انجام شد.");
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
                attachment.AttachmentByAdmin = true;
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
                    AttachmentByAdmin = true,
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
            UserLoanRequestAuthorizationItem entity = (await _userLoanRequestAuthorizationItemService.GetByCondition((UserLoanRequestAuthorizationItem x) => x.Id == id, "Attachments", "ThirdPersonAssuranceAuthorizationItem")).FirstOrDefault();
            if (entity == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            if (entity.ThirdPersonAssuranceAuthorizationItem != null)
            {
                await _thirdPersonAssuranceAuthorizationItemService.Delete(entity.ThirdPersonAssuranceAuthorizationItem);
            }
            if (entity.Attachments?.Any() ?? false)
            {
                foreach (UserLoanRequestAuthorizationItemAttachment attachment in entity.Attachments)
                {
                    _fileHelper.DeleteFile(attachment.Attachment, Directories.AuthorizeItem(_currentUserService.CustomerId.ToString()));
                }
            }
            await _userLoanRequestAuthorizationItemService.Delete(entity);
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
