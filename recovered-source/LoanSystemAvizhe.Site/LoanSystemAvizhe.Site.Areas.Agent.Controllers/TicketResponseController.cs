using System;
using System.IO;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Agent.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class TicketResponseController : BaseAgentController
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
            if (ticket.CreatedById != _currentUserService.UserId && ticket.TargetUserId != _currentUserService.UserId)
            {
                return Error(Guid.Empty, "مجاز به دسترسی به این تیکت نمی باشید.");
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
                FromAdmin = false,
                IsSeen = false,
                Role = TicketRoles.نماینده,
                FileName = fileName,
                TicketId = model.TicketId
            };
            await _ticketResponseService.Insert(entity);
            ticket.Status = TicketStatus.در_انتظار_بررسی;
            ticket.ResponseCount++;
            ticket.LastModifiedAt = DateTime.Now;
            await _ticketService.Update(ticket);
            return Success(entity.Id, "عملیات با موفقیت انجام شد.");
        });
    }
}
