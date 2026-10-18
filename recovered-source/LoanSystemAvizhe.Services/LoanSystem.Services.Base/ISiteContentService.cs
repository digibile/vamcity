using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ISiteContentService
{
    Task<SiteContent> GetById(Guid entityId, params string[] includs);

    Task<List<SiteContent>> GetByCondition(Expression<Func<SiteContent, bool>> condition, params string[] includs);

    Task Insert(SiteContent entity);

    Task Update(SiteContent entity);

    Task Delete(SiteContent entity);
}
