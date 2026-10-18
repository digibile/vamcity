using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;

namespace LoanSystem.Services.Base;

public interface ITicketService
{
    IQueryable<Ticket> GetAllTicketsAsQueryable();

    Task<List<Ticket>> GetByCondition(Expression<Func<Ticket, bool>> condition, params string[] includs);

    Task<List<Ticket>> GetAllTickets();

    Task<Ticket> GetById(Guid entityId, params string[] includs);

    Task<List<GroupedTicketReportModel>> GetGroupedCountByDateAsync(DateTime startDate, DateTime endDate);

    Task InsertBulk(List<Ticket> entities);

    Task Insert(Ticket entity);

    Task Update(Ticket entity);

    Task Delete(Ticket entity);
}
