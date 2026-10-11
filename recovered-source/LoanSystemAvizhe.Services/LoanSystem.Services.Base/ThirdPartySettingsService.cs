using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class ThirdPartySettingsService : BaseService, IThirdPartySettingsService
{
	private readonly AppConfig _appConfig;

	public ThirdPartySettingsService(AppDbContext context, ICurrentUserService currentUserService, IOptions<AppConfig> appConfig)
		: base(context, currentUserService)
	{
		_appConfig = appConfig.Value;
	}

	public Task<List<ThirdPartySettings>> GetByCondition(Expression<Func<ThirdPartySettings, bool>> condition, bool ignoreGlobalFilter, params string[] includs)
	{
		IQueryable<ThirdPartySettings> source = Table<ThirdPartySettings>();
		if (ignoreGlobalFilter)
		{
			source = source.IgnoreQueryFilters();
		}
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<ThirdPartySettings> GetThirdPartySettings()
	{
		return Table<ThirdPartySettings>().FirstOrDefaultAsync((ThirdPartySettings x) => x.CustomerId == (Guid)_currentUserService.CustomerId);
	}

	public Task Update(ThirdPartySettings entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		if (entity.Id == Guid.Empty)
		{
			Insert(entity);
		}
		else
		{
			Update(entity, true);
		}
		return Save();
	}
}
