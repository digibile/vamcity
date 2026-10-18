using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystemAvizhe.Services.ThirdParty;

public interface IIceReportService
{
    IQueryable<IceReport> AsQueryable();

    Task<List<IceReport>> GetAll(Guid? customerId);

    Task<IceReport> GetById(Guid entityId, params string[] includs);

    Task Insert(IceReport entity);

    Task Update(IceReport entity);

    Task Delete(IceReport entity);

    Task<List<IceReport>> GetByCondition(Expression<Func<IceReport, bool>> condition);
}
