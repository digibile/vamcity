using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IAppSettingsService
{
    Task<List<AppSettings>> GetByCondition(Expression<Func<AppSettings, bool>> condition, bool ignoreGlobalFilter, params string[] includs);

    Task<AppSettings> GetAppSettings(Guid customerId);

    Task<AppSettings> GetAppSettings();

    Task Update(AppSettings appSettings);

    Task Insert(AppSettings entity);

    Task SyncChargeSmsCredit(Guid? customerId = null);

    Task SyncChargeInquiryCredit(Guid? customerId = null);
}
