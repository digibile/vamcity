using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Core;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Areas.Admin.Models;
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

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class TicketController : BaseAdminController
{
    private readonly IFileHelper _fileHelper;

    private readonly ITicketService _ticketService;

    private readonly IMemberService _memberService;

    private readonly IAgentService _agentService;

    private readonly IUserLoanService _userLoanService;

    private readonly ICurrentUserService _currentUserService;

    private readonly UserManager _userManager;

    public TicketController(ILogger<TicketController> logger, IFileHelper fileHelper, ITicketService ticketService, IMemberService memberService, IAgentService agentService, IUserLoanService userLoanService, ICurrentUserService currentUserService, UserManager userManager)
        : base(logger)
    {
        _fileHelper = fileHelper;
        _ticketService = ticketService;
        _memberService = memberService;
        _agentService = agentService;
        _userLoanService = userLoanService;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<TicketListResultModel>> List([FromBody] DynamicQueryForTicketModel query)
    {
        return TryCatch(async () =>
        {
            Expression<Func<Ticket, bool>> expression = (Ticket x) => true;
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
                expression = expression.AND((Ticket x) => x.CreatedById == query.MemberId.Value || x.TargetUserId == (long?)query.MemberId.Value);
            }
            if (!string.IsNullOrEmpty(query.Search))
            {
                expression = expression.AND((Ticket x) => x.Title.Contains(query.Search) || x.Code.ToString().Contains(query.Search));
            }
            IQueryable<Ticket> ticketQuery = _ticketService.GetAllTicketsAsQueryable().Include((Ticket x) => x.CreatedBy).Include((Ticket x) => x.TargetUser)
                .Include((Ticket x) => x.UserLoan)
                .Include((Ticket x) => x.UserLoanRequest)
                .Where(expression);
            var statusGroup = await ticketQuery.GroupBy((Ticket x) => x.Status, (TicketStatus key, IEnumerable<Ticket> x) => new
            {
                status = key,
                count = x.Count()
            }).ToListAsync();
            if (query.StatusFilter.HasValue)
            {
                ticketQuery = ticketQuery.Where((Ticket x) => (int?)x.Status == (int?)query.StatusFilter);
            }
            int countAll = await ticketQuery.CountAsync();
            ticketQuery = ((query.StatusFilter != TicketStatus.در_انتظار_بررسی) ? ticketQuery.OrderByDescending((Ticket x) => x.LastModifiedAt).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage) : ticketQuery.OrderBy((Ticket x) => x.CreatedAt).Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage));
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
                Role = x.Role,
                Role_Str = x.Role.ToString().Replace("_", " "),
                Status = x.Status,
                Status_Str = x.Status.ToString().Replace("_", " "),
                ResponseCount = x.ResponseCount,
                CreatedAt = x.CreatedAt.ToPersianDateTime(),
                CreatedById = x.CreatedById,
                LastModifiedAt = x.LastModifiedAt.ToPersianDateTime(),
                CreatedByFullName = x.CreatedBy?.FullName,
                TargetRole = x.TargetRole,
                TargetRole_Str = x.TargetRole.ToString().Replace("_", " "),
                TargetUser = x.TargetUser?.FullName,
                TargetUserId = x.TargetUser?.Id
            }).Cast<object>().ToList();
            TicketListResultModel data = new TicketListResultModel
            {
                Tickets = tickets,
                CountAll = countAll,
                Counts = new
                {
                    Pending = (statusGroup.FirstOrDefault(x => x.status == TicketStatus.در_انتظار_بررسی)?.count ?? 0),
                    Replied = (statusGroup.FirstOrDefault(x => x.status == TicketStatus.پاسخ_داده_شده)?.count ?? 0),
                    Closed = (statusGroup.FirstOrDefault(x => x.status == TicketStatus.بسته_شده)?.count ?? 0),
                    Tracking = (statusGroup.FirstOrDefault(x => x.status == TicketStatus.در_حال_پیگیری)?.count ?? 0),
                    All = statusGroup.Sum(x => x.count)
                }
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
            Ticket ticket = await _ticketService.GetById(id, "TicketResponses.CreatedBy", "UserLoan", "UserLoanRequest", "CreatedBy", "TargetUser");
            if (ticket == null)
            {
                return Error<TicketModel>(null, "اطلاعات مورد نظر یافت نشد");
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
            List<long> userIds = await PrepareUserIds(model.CreateTicketTarget, model.UserIds);
            List<Ticket> tickets = new List<Ticket>();
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
            foreach (long item2 in userIds)
            {
                Ticket item = new Ticket
                {
                    Id = Guid.Empty,
                    Title = new HtmlSanitizer().Sanitize(model.Title),
                    LastModifiedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedById = _currentUserService.UserId.Value,
                    Role = TicketRoles.پشتیبانی_مرکز,
                    TargetUserId = item2,
                    TargetRole = ConvertRole(model.CreateTicketTarget),
                    Status = TicketStatus.پاسخ_داده_شده,
                    ResponseCount = 1,
                    AsMessage = false,
                    TicketResponses = new List<TicketResponse>
                    {
                        new TicketResponse
                        {
                            CreatedAt = DateTime.Now,
                            CreatedById = _currentUserService.UserId.Value,
                            CustomerId = _currentUserService.CustomerId.Value,
                            Description = new HtmlSanitizer().Sanitize(model.Description),
                            FromAdmin = true,
                            Role = TicketRoles.پشتیبانی_مرکز,
                            FileName = fileName
                        }
                    }
                };
                tickets.Add(item);
            }
            if (tickets.Any())
            {
                await _ticketService.InsertBulk(tickets);
            }
            return Success(tickets.Count, "عملیات با موفقیت انجام شد.");
        });
        static TicketRoles ConvertRole(CreateTicketTargets createTicketTarget)
        {
            switch (createTicketTarget)
            {
                case CreateTicketTargets.SuperAdmin:
                    return TicketRoles.مدیر_کل;
                case CreateTicketTargets.AllMembers:
                case CreateTicketTargets.ActiveMembers:
                case CreateTicketTargets.SpecificMembers:
                    return TicketRoles.مشتری;
                case CreateTicketTargets.AllAgents:
                case CreateTicketTargets.SpecificAgents:
                    return TicketRoles.نماینده;
                default:
                    return TicketRoles.نامعلوم;
            }
        }
    }

    private async Task<List<long>> PrepareUserIds(CreateTicketTargets createTicketTarget, List<long> userIds)
    {
        switch (createTicketTarget)
        {
            case CreateTicketTargets.SpecificMembers:
            case CreateTicketTargets.SpecificAgents:
                return userIds;
            case CreateTicketTargets.AllMembers:
                return (await _memberService.GetAllMembers()).Select((User x) => x.Id).ToList();
            case CreateTicketTargets.ActiveMembers:
                return (await _userLoanService.GetNotCheckoutUserLoans()).Select((UserLoan x) => x.UserId).ToList();
            case CreateTicketTargets.AllAgents:
                return (await _agentService.GetAllAgents()).Select((User x) => x.Id).ToList();
            case CreateTicketTargets.SuperAdmin:
                {
                    User user = await _userManager.FindByUserName("SuperAdmin");
                    if (user == null)
                    {
                        throw new CustomException("در حال حاضر امکان ارسال تیکت به ادمین وجود ندارد");
                    }
                    return new List<long> { user.Id };
                }
            default:
                return new List<long>();
        }
    }

    [HttpPost("[action]")]
    public Task<DefaultResponseState> ChangeStatus([FromBody] ChangeTicketStatusModel model)
    {
        return TryCatch(async () =>
        {
            Ticket ticket = await _ticketService.GetById(model.TicketId);
            if (ticket == null)
            {
                return Error("تیکت یافت نشد");
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
            await _ticketService.Delete(ticket);
            return Success();
        });
    }
}
