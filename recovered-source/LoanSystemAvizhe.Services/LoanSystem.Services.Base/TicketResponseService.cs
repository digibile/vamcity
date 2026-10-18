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

public class TicketResponseService : BaseService, ITicketResponseService
{
    public TicketResponseService(AppDbContext context, ICurrentUserService currentUserService)
        : base(context, currentUserService)
    {
    }

    public Task<List<TicketResponse>> GetAllTicketResponses()
    {
        return Table<TicketResponse>().ToListAsync();
    }

    public Task<List<TicketResponse>> GetByTicketId(Guid ticketId, params string[] includs)
    {
        IQueryable<TicketResponse> source = Table<TicketResponse>();
        foreach (string navigationPropertyPath in includs)
        {
            source = source.Include(navigationPropertyPath);
        }
        return source.ToListAsync();
    }

    public Task<TicketResponse> GetById(Guid entityId, params string[] includs)
    {
        return GetById<TicketResponse>(entityId, includs);
    }

    public Task Insert(TicketResponse entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Insert<TicketResponse>(entity);
        return Save();
    }

    public Task Update(TicketResponse entity)
    {
        entity.CustomerId = _currentUserService.CustomerId.Value;
        Update(entity, true);
        return Save();
    }

    public Task Delete(TicketResponse entity)
    {
        Delete<TicketResponse>(entity);
        return Save();
    }
}
