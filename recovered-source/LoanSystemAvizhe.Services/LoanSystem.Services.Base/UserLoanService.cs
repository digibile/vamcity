using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class UserLoanService : BaseService, IUserLoanService
{
    private readonly IAppSmsService _appSmsService;

    private readonly IUserPaymentService _userPaymentService;

    public UserLoanService(AppDbContext context, ICurrentUserService currentUserService, IAppSmsService appSmsService, IUserPaymentService userPaymentService)
        : base(context, currentUserService)
    {
        _appSmsService = appSmsService;
        _userPaymentService = userPaymentService;
    }

    public async Task<List<UserLoan>> Search(string search)
    {
        List<UserLoan> userLoans = await (from x in Table<UserLoan>().Include((UserLoan x) => x.Loan)
                                          where x.Code.ToString() == search
                                          select x).ToListAsync();
        List<User> source = await (from x in _context.Users.AsNoTracking()
                                   where userLoans.Select((UserLoan u) => u.UserId).ToList().Contains(x.Id)
                                   select x).ToListAsync();
        foreach (UserLoan userLoan in userLoans)
        {
            userLoan.User = source.FirstOrDefault((User x) => x.Id == userLoan.UserId);
        }
        return userLoans;
    }

    public Task<List<UserLoan>> GetByCondition(Expression<Func<UserLoan, bool>> condition, params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.AsSplitQuery().ToListAsync();
    }

    public Task<List<UserLoan>> GetAllUserLoans(params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Include((UserLoan x) => x.User);
        source = source.Include((UserLoan x) => x.Loan);
        return source.Where((UserLoan x) => x.User.CustomerId == _currentUserService.CustomerId).ToListAsync();
    }

    public IQueryable<UserLoan> GetAllAsQueryable()
    {
        return Table<UserLoan>();
    }

    public Task<List<UserLoan>> GetUserLoansHasAgent(params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Include((UserLoan x) => x.User);
        return source.Where((UserLoan x) => x.User.CustomerId == _currentUserService.CustomerId && x.AgentId.HasValue).ToListAsync();
    }

    public async Task<List<UserLoan>> GetNotCheckoutUserLoans()
    {
        return (await GetAllUserLoans("UserLoanPayments")).Where((UserLoan x) => !x.IsCheckouted.HasValue || !x.IsCheckouted.Value).ToList();
    }

    public Task<UserLoan> GetById(Guid entityId, params string[] includs)
    {
        return GetById<UserLoan>(entityId, includs);
    }

    public Task<UserLoan> GetByIdNoTrack(Guid entityId, params string[] includs)
    {
        return GetByIdNoTrack<UserLoan>(entityId, includs);
    }

    public Task<List<UserLoan>> GetByUserId(long userId, params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.Where((UserLoan x) => x.UserId == userId).ToListAsync();
    }

    public Task<List<UserLoan>> GetByAgentId(long agentId, params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        source = source.Include((UserLoan x) => x.UserLoanRequest);
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.Where((UserLoan x) => x.AgentId == (long?)agentId).ToListAsync();
    }

    public Task<List<UserLoan>> GetByUserLoanRequestId(Guid userLoanRequestId, params string[] includs)
    {
        IQueryable<UserLoan> source = Table<UserLoan>();
        source = source.Include((UserLoan x) => x.UserLoanRequest);
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.Where((UserLoan x) => x.UserLoanRequestId == userLoanRequestId).ToListAsync();
    }

    public async Task<int> GetLoanCountByDateAsync(DateTime startDate, DateTime endDate)
    {
        return await (from x in _context.UserLoans.Include((UserLoan x) => x.User).AsNoTracking()
                      where x.User.CustomerId == _currentUserService.CustomerId && x.StartDate.Date >= ((DateTime)startDate).Date && x.StartDate.Date <= ((DateTime)endDate).Date
                      select x).CountAsync();
    }

    public async Task SetAppSms(List<UserLoanModel> userLoans, bool ignoreText)
    {
        if (userLoans == null || !userLoans.Any())
        {
            return;
        }
        List<Guid> entityIds = userLoans.Select((UserLoanModel x) => x.Id).ToList();
        List<AppSms> source = await _appSmsService.GetByCondition((AppSms x) => entityIds.Contains(x.EntityId) && x.EntityType == "UserLoan", false);
        foreach (UserLoanModel userLoan in userLoans)
        {
            userLoan.AppSmsList = (from x in source
                                   where x.EntityId == userLoan.Id
                                   select new AppSmsModel(x)).ToList();
            if (ignoreText)
            {
                userLoan.AppSmsList.ForEach((AppSmsModel x) =>
                {
                    x.Text = string.Empty;
                });
            }
        }
    }

    public Task Insert(UserLoan entity)
    {
        Insert<UserLoan>(entity);
        return Save();
    }

    public Task Update(UserLoan entity)
    {
        _context.Entry(entity).Property("Code").IsModified = false;
        Update(entity, true);
        return Save();
    }

    public Task Delete(UserLoan entity)
    {
        IQueryable<UserLoanPayment> entities = _context.UserLoanPayments.Where((UserLoanPayment x) => x.UserLoanId == entity.Id);
        _context.UserLoanPayments.RemoveRange(entities);
        Delete<UserLoan>(entity);
        return Save();
    }

    public async Task<(decimal, decimal)> GetCheckoutAmount(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new CustomException("شناسه وام معتبر نمی باشد.");
        }
        UserLoan userLoan = await GetByIdNoTrack(id, "User", "Loan", "UserLoanPayments");
        if (userLoan == null)
        {
            throw new CustomException("وام یافت نشد.");
        }
        UserLoanModel model = new UserLoanModel(userLoan)
        {
            LoanPayments = userLoan.UserLoanPayments.Select((UserLoanPayment p) => new LoanPaymentModel(p)).ToList()
        };
        await _userPaymentService.SetPaymentsDelays(model.LoanPayments, calcEarly: true);
        decimal val1 = model.Remain;
        await _userPaymentService.SetPaymentsDelays(model.LoanPayments, calcEarly: true, checkoutModeOnLoanProfit: true);
        decimal remain = model.Remain;
        return (val1, remain);
    }
}
