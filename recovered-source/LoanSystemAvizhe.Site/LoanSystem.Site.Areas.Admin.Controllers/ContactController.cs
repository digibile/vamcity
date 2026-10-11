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

namespace LoanSystem.Site.Areas.Admin.Controllers;

[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class ContactController : BaseAdminController
{
	private readonly IContactService _contactService;

	private readonly ICurrentUserService _currentUserService;

	private readonly IUserLoanContactService _userLoanContactService;

	private readonly ITransactionService _transactionService;

	public ContactController(ILogger<ContactController> logger, IContactService contactService, ICurrentUserService currentUserService, IUserLoanContactService userLoanContactService, ITransactionService transactionService)
		: base(logger)
	{
		_contactService = contactService;
		_currentUserService = currentUserService;
		_userLoanContactService = userLoanContactService;
		_transactionService = transactionService;
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<ContactModel>> Get(Guid id)
	{
		return TryCatch(async () =>
		{
			ContactModel data = new ContactModel();
			if (id != Guid.Empty)
			{
				Contact contact = await _contactService.GetById(id);
				if (contact == null)
				{
					return Error<ContactModel>(null, "اطلاعات مورد نظر یافت نشد");
				}
				data = new ContactModel(contact);
			}
			return Success(data);
		});
	}

	[HttpGet("[action]/{calcValues?}")]
	public Task<ResponseState<List<ContactModel>>> List(bool calcValues = false)
	{
		return TryCatch(async () =>
		{
			List<ContactModel> models = (await _contactService.GetByCondition((Contact _) => true)).Select((Contact x) => new ContactModel(x)).ToList();
			if (calcValues && models.Any())
			{
				List<UserLoanContact> userLoanContacts = await _userLoanContactService.GetByCondition((UserLoanContact x) => models.Select((ContactModel m) => m.Id).ToList().Contains(x.ContactId));
				List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => x.ContactId.HasValue && models.Select((ContactModel m) => m.Id).ToList().Contains((Guid)x.ContactId));
				foreach (ContactModel model in models)
				{
					model.LoanCount = userLoanContacts.Count((UserLoanContact x) => x.ContactId == model.Id);
					model.TotalShare = userLoanContacts.Where((UserLoanContact x) => x.ContactId == model.Id).Sum((UserLoanContact x) => x.Share);
					model.TotalReceives = source.Where((Transaction x) => x.ContactId == model.Id).Sum((Transaction x) => x.Amount);
				}
			}
			return Success(models);
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Create([FromBody] ContactModel model)
	{
		return TryCatch(async () =>
		{
			Contact entity = new Contact
			{
				Address = model.Address,
				BusinessName = model.BusinessName,
				FullName = model.FullName,
				BankInfo = model.BankInfo,
				Phone = model.Phone
			};
			await _contactService.Insert(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]")]
	public Task<ResponseState<Guid>> Edit([FromBody] ContactModel model)
	{
		return TryCatch(async () =>
		{
			Contact entity = (await _contactService.GetByCondition((Contact x) => x.Id == model.Id)).FirstOrDefault();
			if (entity == null)
			{
				return Error("آیتم یافت نشد.");
			}
			entity.Address = model.Address;
			entity.Phone = model.Phone;
			entity.BusinessName = model.BusinessName;
			entity.FullName = model.FullName;
			entity.BankInfo = model.BankInfo;
			await _contactService.Update(entity);
			return Success(entity.Id, "عملیات با موفقیت انجام شد.");
		});
	}

	[HttpPost("[action]/{id}")]
	public Task<DefaultResponseState> Delete(Guid id)
	{
		return TryCatch(async () =>
		{
			Contact contact = (await _contactService.GetByCondition((Contact x) => x.Id == id)).FirstOrDefault();
			if (contact == null)
			{
				return Error("امکان حذف وجود ندارد. لطفا بعدا دوباره تلاش نمایید");
			}
			await _contactService.Delete(contact);
			return Success();
		});
	}

	[HttpGet("[action]/{id}")]
	public Task<ResponseState<List<UserLoanContactModel>>> GetLoansByContactId(Guid id)
	{
		return TryCatch(async () =>
		{
			List<UserLoanContactModel> models = new List<UserLoanContactModel>();
			if (id == Guid.Empty)
			{
				return Error<List<UserLoanContactModel>>(null, "کاربر یافت نشد");
			}
			List<UserLoanContact> userLoanContacts = await _userLoanContactService.GetByCondition((UserLoanContact x) => x.ContactId == id, "UserLoan.Loan", "UserLoan.User");
			List<Transaction> source = await _transactionService.GetByCondition((Transaction x) => x.UserLoanContactId.HasValue && x.IsWithdraw && userLoanContacts.Select((UserLoanContact ul) => ul.Id).ToList().Contains((Guid)x.UserLoanContactId));
			foreach (UserLoanContact userLoanContact in userLoanContacts)
			{
				models.Add(new UserLoanContactModel(userLoanContact)
				{
					SumSponsorPayments = source.Where((Transaction pay) => pay.UserLoanContactId == userLoanContact.Id).Sum((Transaction p) => p.Amount)
				});
			}
			return Success(models);
		});
	}
}
