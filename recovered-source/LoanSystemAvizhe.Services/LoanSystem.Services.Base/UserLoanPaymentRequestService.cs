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

public class UserLoanPaymentRequestService : BaseService, IUserLoanPaymentRequestService
{
    private readonly IUserPaymentService _userPaymentService;

    public UserLoanPaymentRequestService(AppDbContext context, ICurrentUserService currentUserService, IUserPaymentService userPaymentService)
        : base(context, currentUserService)
    {
        _userPaymentService = userPaymentService;
    }

    public Task<List<UserLoanPaymentRequest>> GetAll(bool include, Expression<Func<UserLoanPaymentRequest, bool>> condition = null)
    {
        IQueryable<UserLoanPaymentRequest> source = Table<UserLoanPaymentRequest>();
        if (include)
        {
            source = source.Include((UserLoanPaymentRequest x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan);
            source = source.Include((UserLoanPaymentRequest x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User);
        }
        if (condition != null)
        {
            source = source.Where(condition);
        }
        return source.Where((UserLoanPaymentRequest x) => x.UserLoanPayment.UserLoan.User.CustomerId == _currentUserService.CustomerId).AsSplitQuery().ToListAsync();
    }

    public Task<UserLoanPaymentRequest> GetById(Guid entityId, bool include)
    {
        IQueryable<UserLoanPaymentRequest> source = Table<UserLoanPaymentRequest>();
        if (include)
        {
            source = source.Include((UserLoanPaymentRequest x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan);
            source = source.Include((UserLoanPaymentRequest x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.User);
        }
        return source.SingleOrDefaultAsync((UserLoanPaymentRequest x) => x.Id == entityId);
    }

    public Task<List<UserLoanPaymentRequest>> GetByUserId(long userId)
    {
        return (from x in Table<UserLoanPaymentRequest>().Include((UserLoanPaymentRequest x) => x.UserLoanPayment).ThenInclude((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan)
                .Include((UserLoanPaymentRequest x) => x.UserLoanPayment)
                .ThenInclude((UserLoanPayment x) => x.UserLoan)
                .ThenInclude((UserLoan x) => x.User)
                where x.UserLoanPayment.UserLoan.UserId == userId
                select x).ToListAsync();
    }

    public Task Insert(UserLoanPaymentRequest entity)
    {
        Insert<UserLoanPaymentRequest>(entity);
        return Save();
    }

    public async Task Update(UserLoanPaymentRequest entity, bool sendSms = true)
    {
        UserLoanPaymentRequest old = await GetById(entity.Id, include: false);
        base.Update(entity, true);
        await Save();
        if ((old.Status == UserLoanPaymentRequestStatus.در_انتظار_بررسی && entity.Status == UserLoanPaymentRequestStatus.عدم_تایید) || (old.Status == entity.Status && old.AffectOnMainPayment) || (entity.Status == UserLoanPaymentRequestStatus.تایید_شده && !entity.AffectOnMainPayment))
        {
            return;
        }
        UserLoanPayment userLoanPayment = await Table<UserLoanPayment>().Include((UserLoanPayment x) => x.UserLoan).ThenInclude((UserLoan x) => x.Loan).FirstOrDefaultAsync((UserLoanPayment x) => x.Id == entity.UserLoanPaymentId);
        if (userLoanPayment == null)
        {
            return;
        }
        if (entity.Status == UserLoanPaymentRequestStatus.در_انتظار_بررسی)
        {
            throw new CustomException("تغییر وضعیت به 'در انتظار بررسی' مجاز نمی باشد.");
        }
        if (entity.Status == UserLoanPaymentRequestStatus.عدم_تایید && userLoanPayment.SumAmount >= entity.Amount && old.AffectOnMainPayment)
        {
            await _userPaymentService.Delete(userLoanPayment);
        }
        if (entity.Status == UserLoanPaymentRequestStatus.تایید_شده)
        {
            userLoanPayment.Amount += entity.Amount;
            userLoanPayment.SaveDate = entity.PaymentDate;
            UserLoanPayment userLoanPayment2 = userLoanPayment;
            userLoanPayment2.SavedBy = (await _context.Users.FirstOrDefaultAsync((User x) => (long?)x.Id == _currentUserService.UserId))?.FullName;
            userLoanPayment.PaymentMethod = entity.PaymentMethod;
            userLoanPayment2 = userLoanPayment;
            userLoanPayment2.DelayDay = await _userPaymentService.CalcDelayDay(new LoanPaymentModel(userLoanPayment));
            userLoanPayment2 = userLoanPayment;
            userLoanPayment2.DelayPrice = await _userPaymentService.CalcDelayPrice(new LoanPaymentModel(userLoanPayment));
            userLoanPayment.UserLoan = null;
            await _userPaymentService.Update(userLoanPayment, sendSms);
        }
    }

    public async Task Delete(UserLoanPaymentRequest entity)
    {
        Delete<UserLoanPaymentRequest>(entity);
        if (entity.AffectOnMainPayment && entity.Status == UserLoanPaymentRequestStatus.تایید_شده)
        {
            UserLoanPayment userLoanPayment = await _userPaymentService.GetById(entity.UserLoanPaymentId);
            if (userLoanPayment != null)
            {
                userLoanPayment.SumAmount -= entity.Amount;
                base.Update(userLoanPayment, true);
            }
        }
        await Save();
    }
}
