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

public class VisitorLogService : BaseService, IVisitorLogService
{
    public VisitorLogService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<VisitorLog>> GetByCondition(Expression<Func<VisitorLog, bool>> condition, params string[] includs)
    {
        IQueryable<VisitorLog> source = Table<VisitorLog>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        source = source.Where(condition);
        return source.ToListAsync();
    }

    public int GetCountByMrkCode(string mrkCode)
    {
        return Table<VisitorLog>().Count((VisitorLog x) => x.Mrk == mrkCode);
    }

    public Task<List<VisitorLog>> GetAll()
    {
        return Table<VisitorLog>().ToListAsync();
    }

    public Task<VisitorLog> GetById(Guid entityId)
    {
        return Table<VisitorLog>().SingleOrDefaultAsync((VisitorLog x) => x.Id == entityId);
    }

    public Task<List<VisitorLog>> GetByMrkCode(string mrkCode)
    {
        return (from x in Table<VisitorLog>()
                where x.Mrk == mrkCode
                select x).ToListAsync();
    }

    public Task Insert(VisitorLog entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.VisitorLogs.Add(entity);
        return Save();
    }

    public Task Update(VisitorLog entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }
        _context.VisitorLogs.Update(entity);
        return Save();
    }

    public Task Delete(VisitorLog entity)
    {
        _context.VisitorLogs.Remove(entity);
        return Save();
    }
}
