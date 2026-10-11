using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Services.DbContextInterceptors;

public class LoanRequestStatusChangeDbContextInterceptor : ISaveChangesInterceptor, IInterceptor, ILoanRequestStatusChangeDbContextInterceptor
{
	private readonly ILogger<LoanRequestStatusChangeDbContextInterceptor> _logger;

	private readonly IServiceProvider _serviceProvider;

	private readonly ICurrentUserService _currentUserService;

	private List<Guid> _userLoanRequestIds;

	private List<Guid> _userLoanRequestIds_agent;

	public LoanRequestStatusChangeDbContextInterceptor(ILogger<LoanRequestStatusChangeDbContextInterceptor> logger, ICurrentUserService currentUserService, IServiceProvider serviceProvider)
	{
		_logger = logger;
		_currentUserService = currentUserService;
		_userLoanRequestIds = new List<Guid>();
		_userLoanRequestIds_agent = new List<Guid>();
		_serviceProvider = serviceProvider;
	}

	public void SaveChangesFailed(DbContextErrorEventData eventData)
	{
	}

	public Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.CompletedTask;
	}

	public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
	{
		return result;
	}

	public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default(CancellationToken))
	{
		await Saved(eventData);
		return result;
	}

	public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
	{
		eventData.Context.ChangeTracker.DetectChanges();
		_userLoanRequestIds = new List<Guid>();
		_userLoanRequestIds_agent = new List<Guid>();
		foreach (EntityEntry item in from x in eventData.Context.ChangeTracker.Entries()
			where x.Entity is UserLoanRequest
			select x)
		{
			Saving(item, eventData);
		}
		return result;
	}

	public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default(CancellationToken))
	{
		eventData.Context.ChangeTracker.DetectChanges();
		_userLoanRequestIds = new List<Guid>();
		_userLoanRequestIds_agent = new List<Guid>();
		foreach (EntityEntry item in from x in eventData.Context.ChangeTracker.Entries()
			where x.Entity is UserLoanRequest && x.State == EntityState.Modified
			select x)
		{
			Saving(item, eventData);
		}
		return ValueTask.FromResult(result);
	}

	private async Task Saved(DbContextEventData eventData)
	{
		if (!(_userLoanRequestIds?.Any() ?? false) && !(_userLoanRequestIds_agent?.Any() ?? false))
		{
			return;
		}
		List<UserLoanRequest> userLoanRequests = await (from x in (eventData.Context as AppDbContext).UserLoanRequests.Include((UserLoanRequest x) => x.Status).ThenInclude((LoanRequestStatus x) => x.SmsPattern).Include((UserLoanRequest x) => x.AgentStatus)
				.ThenInclude((LoanRequestStatus x) => x.SmsPattern)
				.Include((UserLoanRequest x) => x.Loan)
				.Include((UserLoanRequest x) => x.User)
				.Include((UserLoanRequest x) => x.Agent)
			where _userLoanRequestIds.Contains(x.Id) || _userLoanRequestIds_agent.Contains(x.Id)
			select x).ToListAsync();
		List<UserLoan> userLoans = await (eventData.Context as AppDbContext).UserLoans.Where((UserLoan x) => (x.UserLoanRequestId != null && _userLoanRequestIds.Contains((Guid)x.UserLoanRequestId)) || _userLoanRequestIds_agent.Contains((Guid)x.UserLoanRequestId)).ToListAsync();
		foreach (UserLoanRequest userLoanRequest in userLoanRequests.Where((UserLoanRequest x) => _userLoanRequestIds.Contains(x.Id) && x.Status.SmsPattern != null))
		{
			UserLoanRequestModel model = new UserLoanRequestModel(userLoanRequest);
			UserLoan userLoan = userLoans.FirstOrDefault((UserLoan x) => x.UserLoanRequestId == userLoanRequest.Id);
			if (userLoan != null)
			{
				model.UserLoan = new UserLoanModel(userLoan);
			}
			try
			{
				using IServiceScope scope = _serviceProvider.CreateScope();
				IntegratedSmsSenderService requiredService = scope.ServiceProvider.GetRequiredService<IntegratedSmsSenderService>();
				try
				{
					await requiredService.SendChangeLoanRequestStatus(model);
				}
				catch (Exception)
				{
				}
				IntegratedEmailSenderService requiredService2 = scope.ServiceProvider.GetRequiredService<IntegratedEmailSenderService>();
				try
				{
					await requiredService2.SendChangeLoanRequestStatus(model);
				}
				catch (Exception)
				{
				}
			}
			catch (Exception ex3)
			{
				_logger.LogError("Error Sending Status SMS Notification: " + ex3.Message);
			}
		}
		foreach (UserLoanRequest userLoanRequest2 in userLoanRequests.Where((UserLoanRequest x) => _userLoanRequestIds_agent.Contains(x.Id) && x.AgentStatus?.SmsPattern != null))
		{
			UserLoanRequestModel model = new UserLoanRequestModel(userLoanRequest2);
			UserLoan userLoan2 = userLoans.FirstOrDefault((UserLoan x) => x.UserLoanRequestId == userLoanRequest2.Id);
			if (userLoan2 != null)
			{
				model.UserLoan = new UserLoanModel(userLoan2);
			}
			try
			{
				using (_serviceProvider.CreateScope())
				{
					IntegratedSmsSenderService service = _serviceProvider.GetService<IntegratedSmsSenderService>();
					try
					{
						await service.SendChangeLoanRequestAgentStatus(model);
					}
					catch (Exception)
					{
					}
					IntegratedEmailSenderService service2 = _serviceProvider.GetService<IntegratedEmailSenderService>();
					try
					{
						await service2.SendChangeLoanRequestAgentStatus(model);
					}
					catch (Exception)
					{
					}
				}
			}
			catch (Exception ex6)
			{
				_logger.LogError("Error Sending Agent Status SMS Notification: " + ex6.Message);
			}
		}
	}

	private void Saving(EntityEntry entry, DbContextEventData eventData)
	{
		UserLoanRequest userLoanRequest = entry.Entity as UserLoanRequest;
		if (entry.State == EntityState.Modified && !object.Equals(entry.OriginalValues["StatusId"], userLoanRequest.StatusId))
		{
			_userLoanRequestIds.Add((entry.Entity as UserLoanRequest).Id);
			(eventData.Context as AppDbContext).UserLoanRequestStatusHistories.Add(new UserLoanRequestStatusHistory
			{
				CustomerId = userLoanRequest.CustomerId,
				DateTime = DateTime.Now,
				FromStatusId = Guid.Parse(entry.OriginalValues["StatusId"].ToString()),
				ToStatusId = userLoanRequest.StatusId,
				UserId = _currentUserService.UserId.Value,
				UserLoanRequestId = userLoanRequest.Id
			});
		}
		if (entry.State == EntityState.Modified && !object.Equals(entry.OriginalValues["AgentStatusId"], (entry.Entity as UserLoanRequest).AgentStatusId))
		{
			_userLoanRequestIds_agent.Add((entry.Entity as UserLoanRequest).Id);
			(eventData.Context as AppDbContext).UserLoanRequestStatusHistories.Add(new UserLoanRequestStatusHistory
			{
				CustomerId = userLoanRequest.CustomerId,
				DateTime = DateTime.Now,
				FromAgentStatusId = ((entry.OriginalValues["AgentStatusId"] != null) ? new Guid?(Guid.Parse(entry.OriginalValues["AgentStatusId"].ToString())) : ((Guid?)null)),
				ToAgentStatusId = userLoanRequest.AgentStatusId,
				UserId = _currentUserService.UserId.Value,
				UserLoanRequestId = userLoanRequest.Id
			});
		}
	}
}
