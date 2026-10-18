using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface IThirdPartySettingsService
{
    Task<List<ThirdPartySettings>> GetByCondition(Expression<Func<ThirdPartySettings, bool>> condition, bool ignoreGlobalFilter, params string[] includs);

    Task<ThirdPartySettings> GetThirdPartySettings();

    Task Update(ThirdPartySettings thirdPartySettings);
}
