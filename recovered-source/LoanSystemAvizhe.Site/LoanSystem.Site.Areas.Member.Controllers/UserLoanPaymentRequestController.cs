using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member")]
public class UserLoanPaymentRequestController : BaseMemberController
{
    private readonly IUserLoanService _userLoanService;

    private readonly IUserLoanPaymentRequestService _userLoanPaymentRequestService;

    private readonly IUserPaymentService _userPaymentService;

    private readonly IFileHelper _fileHelper;

    private readonly ICurrentUserService _currentUserService;

    private readonly IAppContext _appContext;

    public UserLoanPaymentRequestController(ILogger<UserLoanPaymentRequestController> logger, IUserLoanPaymentRequestService userLoanPaymentRequestService, IUserPaymentService userPaymentService, IUserLoanService userLoanService, IAppContext appContext, IFileHelper fileHelper, ICurrentUserService currentUserService)
        : base(logger)
    {
        _userLoanPaymentRequestService = userLoanPaymentRequestService;
        _userPaymentService = userPaymentService;
        _appContext = appContext;
        _userLoanService = userLoanService;
        _fileHelper = fileHelper;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<UserLoanPaymentRequestModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<UserLoanPaymentRequestModel>(null, "کاربر یافت نشد");
            }
            if (id == Guid.Empty)
            {
                return Success(new UserLoanPaymentRequestModel
                {
                    PaymentDate = DateTime.Now
                });
            }
            UserLoanPaymentRequest userLoanPaymentRequest = await _userLoanPaymentRequestService.GetById(id, include: true);
            if (userLoanPaymentRequest == null)
            {
                return Error<UserLoanPaymentRequestModel>(null, "اطلاعات مورد نظر یافت نشد.");
            }
            if (userLoanPaymentRequest.UserLoanPayment?.UserLoan?.UserId != _appContext.User.Id)
            {
                return Error<UserLoanPaymentRequestModel>(null, "دسترسی غیرمجاز.");
            }
            UserLoanPaymentRequestModel userLoanPaymentRequestModel = new UserLoanPaymentRequestModel(userLoanPaymentRequest);
            userLoanPaymentRequestModel.FillImageBase64Prop(_fileHelper, _currentUserService.CustomerId.Value);
            return Success(userLoanPaymentRequestModel);
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<UserLoanPaymentRequestModel>>> List()
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<List<UserLoanPaymentRequestModel>>(null, "کاربر یافت نشد");
            }
            List<UserLoanPaymentRequest> source = await _userLoanPaymentRequestService.GetByUserId(_appContext.User.Id);
            List<UserLoanPaymentRequestModel> data = new List<UserLoanPaymentRequestModel>();
            if (source.Any())
            {
                data = source.Select((UserLoanPaymentRequest x) => new UserLoanPaymentRequestModel(x)).ToList();
            }
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create(UserLoanPaymentRequestModel model)
    {
        return TryCatch(async () =>
        {
            DateTime paymentDate = model.PaymentDate;
            if (paymentDate.Hour == 0)
            {
                paymentDate = new DateTime(paymentDate.Year, paymentDate.Month, paymentDate.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            string fileName = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error<Guid>(default, "فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImage(model.File))
                {
                    return Error<Guid>(default, "فقط فایل تصویر مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.UserLoanPaymentRequest, _currentUserService.CustomerId.ToString());
            }
            UserLoanPaymentRequest entity = new UserLoanPaymentRequest
            {
                Amount = model.Amount,
                PaymentDate = paymentDate,
                PaymentMethod = model.PaymentMethod,
                TrackNumber = model.TrackNumber,
                MemberDescription = model.MemberDescription,
                UserLoanPaymentId = model.LoanPayment.Id,
                RequestDate = DateTime.Now,
                Status = UserLoanPaymentRequestStatus.در_انتظار_بررسی,
                FileName = fileName
            };
            await _userLoanPaymentRequestService.Insert(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Edit(UserLoanPaymentRequestModel model)
    {
        return TryCatch(async () =>
        {
            UserLoanPaymentRequest entity = await _userLoanPaymentRequestService.GetById(model.Id, include: true);
            if (entity == null)
            {
                return Error(Guid.Empty, "اطلاعات مورد نظر یافت نشد.");
            }
            if (entity.UserLoanPayment?.UserLoan?.UserId != _appContext.User.Id)
            {
                return Error(Guid.Empty, "دسترسی غیرمجاز.");
            }
            if (entity.Status != UserLoanPaymentRequestStatus.در_انتظار_بررسی)
            {
                return Error(Guid.Empty, "این درخواست به دلیل اینکه توسط مدیر تغییر وضعیت داده شده است قابل ویرایش نمی باشد.");
            }
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error<Guid>(default, "فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImage(model.File))
                {
                    return Error<Guid>(default, "فقط فایل تصویر مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                _fileHelper.DeleteFile(entity.FileName, Directories.UserLoanPaymentRequest(_currentUserService.CustomerId.ToString()));
                entity.FileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.UserLoanPaymentRequest, _currentUserService.CustomerId.ToString());
            }
            else if (!string.IsNullOrEmpty(entity.FileName) && string.IsNullOrEmpty(model.FileName))
            {
                _fileHelper.DeleteFile(entity.FileName, Directories.UserLoanPaymentRequest(_currentUserService.CustomerId.ToString()));
            }
            entity.UserLoanPaymentId = model.LoanPayment.Id;
            entity.Amount = model.Amount;
            entity.MemberDescription = model.MemberDescription;
            entity.PaymentMethod = model.PaymentMethod;
            entity.TrackNumber = model.TrackNumber;
            entity.PaymentDate = model.PaymentDate;
            await _userLoanPaymentRequestService.Update(entity);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            UserLoanPaymentRequest userLoanPaymentRequest = await _userLoanPaymentRequestService.GetById(id, include: true);
            if (userLoanPaymentRequest == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            if (userLoanPaymentRequest.UserLoanPayment?.UserLoan?.UserId != _appContext.User.Id)
            {
                return Error("دسترسی غیرمجاز.");
            }
            if (userLoanPaymentRequest.Status != UserLoanPaymentRequestStatus.در_انتظار_بررسی)
            {
                return Error("این درخواست به دلیل اینکه توسط مدیر تغییر وضعیت داده شده است قابل حذف نمی باشد.");
            }
            userLoanPaymentRequest.UserLoanPayment = null;
            await _userLoanPaymentRequestService.Delete(userLoanPaymentRequest);
            return Success();
        });
    }

    [HttpGet("[action]")]
    public Task<ResponseState<List<UserLoanModel>>> LoansList()
    {
        return TryCatch(async () =>
        {
            if (_appContext.User == null)
            {
                return Error<List<UserLoanModel>>(null, "کاربر یافت نشد");
            }
            List<UserLoan> source = await _userLoanService.GetByUserId(_appContext.User.Id, "Loan", "User", "UserLoanPayments");
            List<UserLoanModel> data = new List<UserLoanModel>();
            if (source.Any())
            {
                data = (from x in source
                        select new UserLoanModel
                        {
                            Id = x.Id,
                            Code = x.Code,
                            LoanModel = new Loan2Model
                            {
                                Id = x.Loan.Id,
                                Title = x.Loan.Title
                            },
                            Amount = x.Amount,
                            LoanPayments = x.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
                        } into x
                        orderby x.StartDate descending
                        select x).ToList();
            }
            return Success(data);
        });
    }
}
