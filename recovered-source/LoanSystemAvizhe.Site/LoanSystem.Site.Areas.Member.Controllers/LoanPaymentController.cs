using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Site.Helpers.Shared;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Areas.Member.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Member,Admin")]
public class LoanPaymentController : BaseMemberController
{
	private readonly IUserPaymentService _userPaymentService;

	private readonly ICurrentUserService _currentUserService;

	public LoanPaymentController(ILogger<LoanPaymentController> logger, IUserPaymentService userPaymentService, ICurrentUserService currentUserService)
		: base(logger)
	{
		_userPaymentService = userPaymentService;
		_currentUserService = currentUserService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<LoanPaymentModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			UserLoanPayment userLoanPayment = await _userPaymentService.GetById(id, "UserLoan", "UserLoanPaymentOthers");
			if (userLoanPayment == null)
			{
				return Error<LoanPaymentModel>(null, "قسط یافت نشد.");
			}
			if (userLoanPayment.UserLoan?.UserId != _currentUserService.UserId)
			{
				return Error<LoanPaymentModel>(null, "دسترسی به اطلاعات برای شما مجاز نمی باشد");
			}
			LoanPaymentModel model = new LoanPaymentModel(userLoanPayment);
			await _userPaymentService.SetPaymentsDelays(new List<LoanPaymentModel> { model }, calcEarly: true);
			return Success(model, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<LoanPaymentModel>>> GetLoanPayments(Guid id)
	{
		return TryCatch(async () =>
		{
			List<LoanPaymentModel> models = new List<LoanPaymentModel>();
			if (id == Guid.Empty)
			{
				return Error<List<LoanPaymentModel>>(null, "وام یافت نشد.");
			}
			List<UserLoanPayment> list = await _userPaymentService.GetPaymentsByUserLoanId(id, includeMemberPurchases: true, includePaymentRequest: true);
			if (list.Any())
			{
				if (!_currentUserService.IsInRole("Admin") && list.First().UserLoan.Loan.HasSponsore)
				{
					list.Clear();
				}
				models = (from x in list
					orderby x.TargetMonth
					select new LoanPaymentModel(x)).ToList();
				await _userPaymentService.SetPaymentsDelays(models);
				await _userPaymentService.SetAppSms(models, ignoreText: true);
			}
			return Success(models);
		});
	}
}
