using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.EntityFrameworkCore;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class InquirySettingsService : BaseService, IInquirySettingsService
{
	public InquirySettingsService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public Task<int> GetCountByCondition(Expression<Func<InquirySettings, bool>> condition)
	{
		return Table<InquirySettings>().Where(condition).CountAsync();
	}

	public Task<List<InquirySettings>> GetByCondition(Expression<Func<InquirySettings, bool>> condition)
	{
		return Table<InquirySettings>().Where(condition).ToListAsync();
	}

	public Task<List<InquirySettings>> GetAll()
	{
		return Table<InquirySettings>().ToListAsync();
	}

	public Task<InquirySettings> GetById(Guid entityId, params string[] includs)
	{
		return GetById<InquirySettings>(entityId, includs);
	}

	public async Task Insert(InquirySettings entity)
	{
		Insert<InquirySettings>(entity);
		await Save();
	}

	public Task Update(InquirySettings entity)
	{
		Update(entity, true);
		return Save();
	}

	public Task Delete(InquirySettings entity)
	{
		Delete<InquirySettings>(entity);
		return Save();
	}
}
