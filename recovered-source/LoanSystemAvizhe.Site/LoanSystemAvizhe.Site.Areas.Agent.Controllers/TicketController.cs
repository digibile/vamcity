using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Areas.Admin.Models;
using LoanSystem.Site.Areas.Agent.Controllers;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.Infrastructure;
using LoanSystemAvizhe.Services.Models.DynamicQueryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Site.Areas.Agent.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Agent")]
public class TicketController : BaseAgentController
{
    private readonly IFileHelper _fileHelper;

    private readonly ITicketService _ticketService;

    private readonly ICurrentUserService _currentUserService;

    public TicketController(ILogger<TicketController> logger, IFileHelper fileHelper, ITicketService ticketService, ICurrentUserService currentUserService)
        : base(logger)
    {
        _fileHelper = fileHelper;
        _ticketService = ticketService;
        _currentUserService = currentUserService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<TicketListResultModel>> List([FromBody] DynamicQueryForTicketModel query)
    {
        return TryCatch(async () =>
        {
            Expression<Func<Ticket, bool>> expression = (Ticket x) => (long?)x.CreatedById == _currentUserService.UserId || x.TargetUserId == _currentUserService.UserId;
            if (query.StartDate.HasValue)
            {
                expression = expression.AND((Ticket x) => x.CreatedAt.Date >= query.StartDate.Value.Date);
            }
            if (query.EndDate.HasValue)
            {
                expression = expression.AND((Ticket x) => x.CreatedAt.Date <= query.EndDate.Value.Date);
            }
            if (query.MemberId.HasValue)
            {
                expression = expression.AND((Ticket x) => x.CreatedById == query.MemberId.Value);
            }
            if (!string.IsNullOrEmpty(query.Search))
            {
                expression = expression.AND((Ticket x) => x.Title.Contains(query.Search) || x.Code.ToString().Contains(query.Search));
            }
            IQueryable<Ticket> ticketQuery = _ticketService.GetAllTicketsAsQueryable().Include((Ticket x) => x.CreatedBy).Include((Ticket x) => x.TargetUser)
                .Include((Ticket x) => x.UserLoan)
                .Include((Ticket x) => x.UserLoanRequest)
                .Where(expression);
            int countAll = await ticketQuery.CountAsync();
            ticketQuery = ticketQuery.OrderByDescending((Ticket x) => x.CreatedAt).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage);
            List<object> tickets = (await ticketQuery.ToListAsync()).Select((Ticket x) => new
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                AsMessage = x.AsMessage,
                UserLoanCode = x.UserLoan?.Code,
                UserLoanId = x.UserLoanId,
                UserLoanRequestCode = x.UserLoanRequest?.Code,
                UserLoanRequestId = x.UserLoanRequestId,
                Role_Str = x.Role.ToString().Replace("_", " "),
                Status = x.Status,
                Status_Str = x.Status.ToString().Replace("_", " "),
                ResponseCount = x.ResponseCount,
                CreatedAt = x.CreatedAt.ToPersianDateTime(),
                CreatedById = x.CreatedById,
                CreatedByFullName = x.CreatedBy?.FullName,
                TargetRole_Str = x.TargetRole.ToString().Replace("_", " "),
                TargetUser = x.TargetUser?.FullName
            }).Cast<object>().ToList();
            TicketListResultModel data = new TicketListResultModel
            {
                Tickets = tickets,
                CountAll = countAll
            };
            return Success(data);
        });
    }

    [HttpGet("[action]/{id}")]
    public Task<ResponseState<TicketModel>> Get(Guid id)
    {
        return TryCatch(async () =>
        {
            if (id == Guid.Empty)
            {
                return Success(new TicketModel
                {
                    CreatedAt = DateTime.Now
                });
            }
            Ticket ticket = await _ticketService.GetById(id, "TicketResponses.CreatedBy", "UserLoan", "UserLoanRequest", "CreatedBy");
            if (ticket == null)
            {
                return Error<TicketModel>(null, "اطلاعات مورد نظر یافت نشد");
            }
            if (ticket.CreatedById != _currentUserService.UserId && ticket.TargetUserId != _currentUserService.UserId)
            {
                return Error<TicketModel>(null, "مجاز به دسترسی به این تیکت نمی باشید.");
            }
            TicketModel ticketModel = new TicketModel(ticket);
            ticketModel.TicketResponses = ticketModel.TicketResponses.OrderByDescending((TicketResponseModel x) => x.CreatedAt).ToList();
            ticketModel.TicketResponses.ForEach((TicketResponseModel r) =>
            {
                r.FillFileProps(_fileHelper, _currentUserService.CustomerId.Value);
            });
            return Success(ticketModel);
        });
    }

    [HttpPost("[action]")]
    public Task<ResponseState<int>> Create(CreateTicketModel model)
    {
        return TryCatch(async () =>
        {
            string fileName = "";
            if (model.File != null)
            {
                if (model.File.Length == 0L)
                {
                    return Error(0, "فایل خالی است.");
                }
                _ = model.File.Length;
                if (!_fileHelper.CheckFileIsImageOrPdfOrRarOrZipOrXls(model.File))
                {
                    return Error(0, "فقط فایل تصویر و pdf و zip مجاز می باشد.");
                }
                Path.GetExtension(model.File.FileName);
                fileName = await _fileHelper.UploadFile(model.File, FileUploadDirectory.Ticket, _currentUserService.CustomerId.ToString());
            }
            Ticket ticketEntity = new Ticket
            {
                Id = Guid.Empty,
                Title = new HtmlSanitizer().Sanitize(model.Title),
                CreatedAt = DateTime.Now,
                LastModifiedAt = DateTime.Now,
                CreatedById = _currentUserService.UserId.Value,
                Role = TicketRoles.نماینده,
                TargetRole = TicketRoles.پشتیبانی_مرکز,
                Status = TicketStatus.در_انتظار_بررسی,
                ResponseCount = 1,
                AsMessage = model.AsMessage,
                UserLoanId = model.UserLoanId,
                UserLoanRequestId = model.UserLoanRequestId,
                TicketResponses = new List<TicketResponse>
                {
                    new TicketResponse
                    {
                        CreatedAt = DateTime.Now,
                        CreatedById = _currentUserService.UserId.Value,
                        CustomerId = _currentUserService.CustomerId.Value,
                        Description = new HtmlSanitizer().Sanitize(model.Description),
                        FromAdmin = false,
                        Role = TicketRoles.نماینده,
                        FileName = fileName
                    }
                }
            };
            await _ticketService.Insert(ticketEntity);
            return Success(ticketEntity.Code, "عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> ChangeStatus([FromBody] ChangeTicketStatusModel model)
    {
        return TryCatch(async () =>
        {
            if (model.Status != TicketStatus.بسته_شده)
            {
                return Error("فقط مجاز به بستن تیکت هستید.");
            }
            Ticket ticket = await _ticketService.GetById(model.TicketId);
            if (ticket == null)
            {
                return Error("تیکت یافت نشد");
            }
            if (ticket.CreatedById != _currentUserService.UserId && ticket.TargetUserId != _currentUserService.UserId)
            {
                return Error("مجاز به دسترسی به این تیکت نمی باشید.");
            }
            ticket.Status = model.Status;
            ticket.LastModifiedAt = DateTime.Now;
            await _ticketService.Update(ticket);
            return Success("عملیات با موفقیت انجام شد.");
        });
    }

    [HttpPost("[action]/{id}")]
    public Task<DefaultResponseState> Delete(Guid id)
    {
        return TryCatch(async () =>
        {
            Ticket ticket = await _ticketService.GetById(id);
            if (ticket == null)
            {
                return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
            }
            if (ticket.CreatedById != _currentUserService.UserId && ticket.TargetUserId != _currentUserService.UserId)
            {
                return Error("مجاز به دسترسی به این تیکت نمی باشید.");
            }
            if (ticket.Status != TicketStatus.در_انتظار_بررسی)
            {
                return Error("در وضعیتی که تیکت قرار دارد مجاز به حذف تیکت نمی باشید.");
            }
            await _ticketService.Delete(ticket);
            return Success();
        });
    }
}
