using System;
using System.Threading.Tasks;
using Ganss.Xss;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Users;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsultantController : BaseDefaultController
{
    private readonly UserManager _userManager;

    private readonly IConsultantService _consultantService;

    private readonly IntegratedSmsSenderService _integratedSmsSenderService;

    private readonly IntegratedEmailSenderService _integratedEmailSenderService;

    public ConsultantController(ILogger<AccountController> logger, UserManager userManager, IConsultantService consultantService, IntegratedSmsSenderService integratedSmsSenderService, IntegratedEmailSenderService integratedEmailSenderService)
        : base(logger)
    {
        _userManager = userManager;
        _consultantService = consultantService;
        _integratedSmsSenderService = integratedSmsSenderService;
        _integratedEmailSenderService = integratedEmailSenderService;
    }

    [HttpPost("[action]")]
    public Task<ResponseState<Guid>> Create([FromBody] ConsultantModel model)
    {
        return TryCatch(async () =>
        {
            HtmlSanitizer htmlSanitizer = new HtmlSanitizer();
            Consultant consultant = new Consultant
            {
                Name = htmlSanitizer.Sanitize(model.Name),
                Description = htmlSanitizer.Sanitize(model.Description),
                Phone = htmlSanitizer.Sanitize(model.Phone),
                SaveDate = DateTime.Now
            };
            await _consultantService.Insert(consultant);
            try
            {
                await _integratedSmsSenderService.SendNewConsultantNotification(model.Name);
            }
            catch (Exception)
            {
            }
            try
            {
                await _integratedEmailSenderService.SendNewConsultantNotification(model.Name);
            }
            catch (Exception)
            {
            }
            return Success(consultant.Id, "عملیات با موفقیت انجام شد.");
        });
    }

    private bool CheckMobileFormatValidity(string mobile)
    {
        if (string.IsNullOrEmpty(mobile))
        {
            return false;
        }
        long result2;
        if (!mobile.StartsWith("0"))
        {
            if (mobile.Length != 10 || !long.TryParse(mobile, out var _))
            {
                return false;
            }
        }
        else if (mobile.Length != 11 || !long.TryParse(mobile, out result2))
        {
            return false;
        }
        return true;
    }
}
