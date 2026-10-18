using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class PurchaseService : BaseService, IPurchaseService
{
    private readonly ICustomerService _customerService;

    private readonly IMarketerService _marketerService;

    private readonly IAffiliateCommissionService _affiliateCommissionService;

    private readonly ICustomerPurchasePlanService _customerPurchasePlanService;

    private readonly IAppSettingsService _appSettingsService;

    public PurchaseService(AppDbContext context, ICurrentUserService currentUserService, ICustomerService customerService, IAppSettingsService appSettingsService, IMarketerService marketerService, IAffiliateCommissionService affiliateCommissionService, ICustomerPurchasePlanService customerPurchasePlanService)
        : base(context, currentUserService)
    {
        _customerService = customerService;
        _appSettingsService = appSettingsService;
        _marketerService = marketerService;
        _affiliateCommissionService = affiliateCommissionService;
        _customerPurchasePlanService = customerPurchasePlanService;
    }

    public IQueryable<Purchase> GetAllAsQueryable()
    {
        return Table<Purchase>();
    }

    public Task<List<Purchase>> GetAll(params string[] includs)
    {
        IQueryable<Purchase> source = Table<Purchase>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.Where((Purchase x) => x.CustomerId == _currentUserService.CustomerId).ToListAsync();
    }

    public Task<Purchase> GetById(Guid entityId, params string[] includs)
    {
        return GetById<Purchase>(entityId, includs);
    }

    public Task<Purchase> GetByZarinPalAuthority(string authority, params string[] includs)
    {
        IQueryable<Purchase> source = Table<Purchase>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.SingleOrDefaultAsync((Purchase x) => x.ZarinPalAuthority == authority);
    }

    public async Task ApplyPurchase(Purchase purchase)
    {
        if (purchase == null)
        {
            throw new CustomException("سفارش یافت نشد.");
        }
        if (purchase.PaymentStep == PaymentSteps.Finish)
        {
            throw new CustomException("سفارش قبلا اعمال شده است.");
        }
        if (!purchase.IsPaymentSuccess)
        {
            throw new CustomException("سفارش دارای پرداخت موفق نمی باشد.");
        }
        switch (purchase.ProductType)
        {
            case ProductTypes.Permanent:
                {
                    Customer customer = purchase.Customer;
                    if (customer != null)
                    {
                        CustomerPurchasePlan customerPurchasePlan = new CustomerPurchasePlan
                        {
                            CustomerId = customer.Id,
                            MaxLoanCount = 1000000,
                            PurchaseId = purchase.Id,
                            RemainLoanCount = 1000000,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddYears(1)
                        };
                        await _customerPurchasePlanService.Insert(customerPurchasePlan);
                        customer.Purchased = true;
                        customer.ExpireDate = customerPurchasePlan.EndDate;
                        customer.Permanent = true;
                        await _customerService.Update(customer);
                    }
                    purchase.PaymentStep = PaymentSteps.Finish;
                    await Update(purchase);
                    string affCode = purchase.AffCode;
                    if (string.IsNullOrEmpty(affCode) && !string.IsNullOrEmpty(customer.AffCode))
                    {
                        affCode = customer.AffCode;
                    }
                    if (!string.IsNullOrEmpty(affCode))
                    {
                        User user2 = await _marketerService.GetByAffliateCode(affCode);
                        if (user2 != null)
                        {
                            AffiliateCommission entity2 = new AffiliateCommission
                            {
                                Amount = (int)(purchase.Amount * (decimal)user2.CommissionPercent / 100m),
                                Date = DateTime.Now,
                                Description = purchase.Description,
                                PurchaseId = purchase.Id,
                                UserId = user2.Id
                            };
                            await _affiliateCommissionService.Insert(entity2);
                        }
                    }
                    break;
                }
            case ProductTypes.Cloud:
                {
                    Customer customer = purchase.Customer;
                    if (customer != null)
                    {
                        CustomerPurchasePlan customerPurchasePlan = new CustomerPurchasePlan
                        {
                            CustomerId = customer.Id,
                            MaxLoanCount = purchase.LoanCount,
                            PurchaseId = purchase.Id,
                            RemainLoanCount = purchase.LoanCount,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddYears(1)
                        };
                        await _customerPurchasePlanService.Insert(customerPurchasePlan);
                        customer.Purchased = true;
                        customer.ExpireDate = customerPurchasePlan.EndDate;
                        customer.Permanent = false;
                        await _customerService.Update(customer);
                    }
                    purchase.PaymentStep = PaymentSteps.Finish;
                    await Update(purchase);
                    string affCode = purchase.AffCode;
                    if (string.IsNullOrEmpty(affCode) && !string.IsNullOrEmpty(customer.AffCode))
                    {
                        affCode = customer.AffCode;
                    }
                    if (!string.IsNullOrEmpty(affCode))
                    {
                        User user = await _marketerService.GetByAffliateCode(affCode);
                        if (user != null)
                        {
                            AffiliateCommission entity = new AffiliateCommission
                            {
                                Amount = (int)(purchase.Amount * (decimal)user.CommissionPercent / 100m),
                                Date = DateTime.Now,
                                Description = purchase.Description,
                                PurchaseId = purchase.Id,
                                UserId = user.Id
                            };
                            await _affiliateCommissionService.Insert(entity);
                        }
                    }
                    break;
                }
            case ProductTypes.SMS:
                await _appSettingsService.SyncChargeSmsCredit(purchase.CustomerId);
                purchase.PaymentStep = PaymentSteps.Finish;
                await Update(purchase);
                break;
            case ProductTypes.Inquiry:
                await _appSettingsService.SyncChargeInquiryCredit(purchase.CustomerId);
                purchase.PaymentStep = PaymentSteps.Finish;
                await Update(purchase);
                break;
        }
    }

    public Task Insert(Purchase entity)
    {
        Insert<Purchase>(entity);
        return Save();
    }

    public Task Update(Purchase entity)
    {
        Update(entity, true);
        return Save();
    }

    public Task Delete(Purchase entity)
    {
        Delete<Purchase>(entity);
        return Save();
    }
}
