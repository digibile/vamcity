using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class UserSavedMoneyService : BaseService, IUserSavedMoneyService
{
    private readonly IAppSettingsService _appSettingsService;

    public UserSavedMoneyService(AppDbContext context, IAppSettingsService appSettingsService, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
        _appSettingsService = appSettingsService;
    }

    public Task<List<UserSavedMoney>> GetAll(bool includeDetails = false)
    {
        return (from x in Table<UserSavedMoney>().Include((UserSavedMoney x) => x.Member)
                where x.Member.CustomerId == _currentUserService.CustomerId
                select x).ToListAsync();
    }

    public Task<UserSavedMoney> GetById(Guid entityId, params string[] includs)
    {
        return (from x in Table<UserSavedMoney>().Include((UserSavedMoney x) => x.Member)
                where x.Member.CustomerId == _currentUserService.CustomerId && x.Id == entityId
                select x).FirstOrDefaultAsync();
    }

    public Task<List<UserSavedMoney>> GetSavedMoneiesByUserId(Guid userLoanId)
    {
        return (from x in Table<UserSavedMoney>().Include((UserSavedMoney x) => x.Member)
                where x.Member.CustomerId == _currentUserService.CustomerId
                select x).ToListAsync();
    }
}
