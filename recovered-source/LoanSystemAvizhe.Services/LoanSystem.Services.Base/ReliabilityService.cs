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
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ReliabilityService : BaseService, IReliabilityService
{
    private readonly IMemberService _memberService;

    private IUserPaymentService _userPaymentService;

    public ReliabilityService(AppDbContext context, ICurrentUserService currentUserService, IMemberService memberService)
        : base(context, currentUserService)
    {
        _memberService = memberService;
    }

    public void SetUserPaymentServie(IUserPaymentService userPaymentService)
    {
        _userPaymentService = userPaymentService;
    }

    public Task<List<Reliability>> GetByCondition(Expression<Func<Reliability, bool>> condition)
    {
        return Table<Reliability>().Where(condition).ToListAsync();
    }

    public Task<List<Reliability>> GetAll(Guid? customerId)
    {
        IQueryable<Reliability> source = Table<Reliability>();
        if (customerId.HasValue)
        {
            source = from x in source.IgnoreQueryFilters()
                     where x.CustomerId == customerId
                     select x;
        }
        return source.ToListAsync();
    }

    public Task<Reliability> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Reliability>(entityId, includs);
    }

    public async Task Insert(Reliability entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<Reliability>(entity);
        await Save();
    }

    public Task Update(Reliability entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(Reliability entity)
    {
        Delete<Reliability>(entity);
        return Save();
    }

    public async Task SetMembersReliability(List<long> memberIds, Dictionary<long, List<UserLoanPayment>> member_payments_dic)
    {
        List<Reliability> allReliabilities = await GetAll(null);
        if (allReliabilities == null || !allReliabilities.Any())
        {
            return;
        }
        try
        {
            Dictionary<long, List<UserLoanPayment>> dictionary = member_payments_dic;
            if (dictionary == null || !dictionary.Any())
            {
                return;
            }
            List<User> members = await _memberService.GetByCondition((User x) => member_payments_dic.Select((KeyValuePair<long, List<UserLoanPayment>> keyValuePair) => keyValuePair.Key).ToList().Contains(x.Id));
            foreach (KeyValuePair<long, List<UserLoanPayment>> member_dic in member_payments_dic)
            {
                Reliability reliability = await ProcessPaymentReliability(member_dic.Value, allReliabilities);
                User user = members.FirstOrDefault((User x) => x.Id == member_dic.Key);
                if (user != null)
                {
                    user.ReliabilityId = reliability?.Id;
                }
            }
            await _memberService.Update(members);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task SetMemberReliability(long memberId, Guid reliabilityId)
    {
        User user = await _memberService.GetById(memberId);
        if (user == null)
        {
            throw new CustomException("مشتری یافت نشد.");
        }
        user.ReliabilityId = reliabilityId;
        await _memberService.Update(user);
    }

    private async Task<Reliability> ProcessPaymentReliability(List<UserLoanPayment> userLoanPayments, List<Reliability> allReliabilities)
    {
        if (allReliabilities != null && allReliabilities.Any())
        {
            if (userLoanPayments != null && userLoanPayments.Any())
            {
                Reliability final_reliability = null;
                List<LoanPaymentModel> paymentModels = userLoanPayments.Select((UserLoanPayment x) => new LoanPaymentModel(x)).ToList();
                foreach (LoanPaymentModel item in paymentModels)
                {
                    LoanPaymentModel loanPaymentModel = item;
                    loanPaymentModel.DelayDay = await _userPaymentService.CalcDelayDay(item);
                }
                foreach (Reliability reliability in allReliabilities)
                {
                    IQueryable<LoanPaymentModel> source = paymentModels.AsQueryable();
                    if (reliability.From > 0)
                    {
                        source = source.Where((LoanPaymentModel x) => x.DelayDay >= reliability.From);
                    }
                    if (reliability.To > 0)
                    {
                        source = source.Where((LoanPaymentModel x) => x.DelayDay <= reliability.To);
                    }
                    int num = source.Count();
                    switch (reliability.RowType)
                    {
                        case ReliabilityRowTypes.همه_اقساط:
                            if (num == paymentModels.Count)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداکثر_یک_چهارم_اقساط:
                            if (num <= paymentModels.Count * 25 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداکثر_دو_چهارم_اقساط:
                            if (num <= paymentModels.Count * 50 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداکثر_سه_چهارم_اقساط:
                            if (num <= paymentModels.Count * 75 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداقل_یک_چهارم_اقساط:
                            if (num >= paymentModels.Count * 25 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداقل_دو_چهارم_اقساط:
                            if (num >= paymentModels.Count * 50 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.حداقل_سه_چهارم_اقساط:
                            if (num >= paymentModels.Count * 75 / 100)
                            {
                                final_reliability = reliability;
                            }
                            break;
                        case ReliabilityRowTypes.تعداد_مشخص:
                            if (num == reliability.Count)
                            {
                                final_reliability = reliability;
                            }
                            break;
                    }
                    if (final_reliability != null)
                    {
                        break;
                    }
                }
                return final_reliability;
            }
        }
        return null;
    }
}
