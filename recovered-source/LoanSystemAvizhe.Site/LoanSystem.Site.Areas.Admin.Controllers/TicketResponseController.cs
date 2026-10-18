using System;
using System.IO;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class TicketResponseController : BaseAdminController
{
    private readonly IFileHelper _fileHelper;

    private readonly ITicketResponseService _ticketResponseService;

    private readonly ITicketService _ticketService;

    private readonly ICurrentUserService _currentUserService;

    public TicketResponseController(ILogger<TicketResponseController> logger, IFileHelper fileHelper, ITicketResponseService ticketResponseService, ITicketService ticketService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _fileHelper = fileHelper;
        _ticketResponseService = ticketResponseService;
        _ticketService = ticketService;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<TicketResponseModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (id == Guid.Empty)
            {
                return Success(new TicketResponseModel
                {
                    CreatedAt = DateTime.Now
                });
            }
            TicketResponse ticketResponse = await _ticketResponseService.GetById(id);
            if (ticketResponse == null)
            {
                return Error<TicketResponseModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            TicketResponseModel data = new TicketResponseModel(ticketResponse);
            return Success(data);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create(TicketResponseModel model)
    {
        return TryCatch(async () =>
        {
            Ticket ticket = await _ticketService.GetById(model.TicketId);
            if (ticket == null)
            {
                return Error("تیکت یافت نشد");
            }
            string fileName = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error(Guid.Empty, "فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.File))
                {
                    return Error(Guid.Empty, "فقط فایل تصویر و pdf و zip مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Ticket, _currentUserService.CustomerId.ToString());
            }
            TicketResponse entity = new TicketResponse
            {
                CreatedAt = DateTime.Now,
                CreatedById = _currentUserService.UserId.Value,
                CustomerId = _currentUserService.CustomerId,
                Description = new HtmlSanitizer().Sanitize(model.Description),
                FromAdmin = true,
                IsSeen = false,
                Role = TicketRoles.پشتیبانی_مرکز,
                FileName = fileName,
                TicketId = model.TicketId
            };
            await _ticketResponseService.Insert(entity);
            ticket.Status = TicketStatus.پاسخ_داده_شده;
            ticket.ResponseCount++;
            ticket.LastModifiedAt = DateTime.Now;
            await _ticketService.Update(ticket);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> Edit(TicketResponseModel model)
    {
        return TryCatch(async () =>
        {
            TicketResponse ticketResponse = await _ticketResponseService.GetById(model.Id);
            if (ticketResponse == null)
            {
                return Error("آیتم 'پاسخ تیکت' یافت نشد");
            }
            string fileName = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error("فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.File))
                {
                    return Error("فقط فایل تصویر و pdf و zip مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Ticket, _currentUserService.CustomerId.ToString());
            }
            ticketResponse.CreatedById = _currentUserService.UserId.Value;
            ticketResponse.Description = new HtmlSanitizer().Sanitize(model.Description);
            ticketResponse.FileName = fileName;
            await _ticketResponseService.Update(ticketResponse);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            TicketResponse ticketResponse = await _ticketResponseService.GetById(id);
            if (ticketResponse == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            Ticket ticket = await _ticketService.GetById(ticketResponse.TicketId);
            await _ticketResponseService.Delete(ticketResponse);
            ticket.ResponseCount--;
            await _ticketService.Update(ticket);
            return Success();
        });
    }
}
