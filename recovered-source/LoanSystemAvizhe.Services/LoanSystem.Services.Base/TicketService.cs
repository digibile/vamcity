using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.EntityFrameworkCore;
using SharpCompress;
using Spm.Services.Infrastructure;

namespace LoanSystem.Services.Base;

public class TicketService : BaseService, ITicketService
{
	public TicketService(AppDbContext context, ICurrentUserService currentUserService)
		: base(context, currentUserService)
	{
	}

	public IQueryable<Ticket> GetAllTicketsAsQueryable()
	{
		return Table<Ticket>();
	}

	public Task<List<Ticket>> GetByCondition(Expression<Func<Ticket, bool>> condition, params string[] includs)
	{
		IQueryable<Ticket> source = Table<Ticket>();
		foreach (string navigationPropertyPath in includs)
		{
			source = source.Include(navigationPropertyPath);
		}
		source = source.Where(condition);
		return source.ToListAsync();
	}

	public Task<List<Ticket>> GetAllTickets()
	{
		return Table<Ticket>().ToListAsync();
	}

	public Task<Ticket> GetById(Guid entityId, params string[] includs)
	{
		return GetById<Ticket>(entityId, includs);
	}

	public Task InsertBulk(List<Ticket> entities)
	{
		entities.ForEach((Ticket x) =>
		{
			x.CustomerId = _currentUserService.CustomerId;
			ICollection<TicketResponse> ticketResponses = x.TicketResponses;
			if (ticketResponses != null && ticketResponses.Any())
			{
				x.TicketResponses.ForEach((TicketResponse r) =>
				{
					r.CustomerId = _currentUserService.CustomerId;
				});
			}
		});
		InsertBulk<Ticket>(entities);
		return Save();
	}

	public Task<List<GroupedTicketReportModel>> GetGroupedCountByDateAsync(DateTime startDate, DateTime endDate)
	{
		return _context.Tickets.Where((Ticket x) => x.CreatedAt.Date >= startDate && x.CreatedAt.Date <= endDate && (int)x.Role != 1 && x.CustomerId == _currentUserService.CustomerId).GroupBy((Ticket x) => x.Status, (TicketStatus key, IEnumerable<Ticket> x) => new GroupedTicketReportModel
		{
			Status = key,
			Count = x.Count()
		}).ToListAsync();
	}

	public Task Insert(Ticket entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		ICollection<TicketResponse> ticketResponses = entity.TicketResponses;
		if (ticketResponses != null && ticketResponses.Any())
		{
			entity.TicketResponses.ForEach((TicketResponse r) =>
			{
				r.CustomerId = _currentUserService.CustomerId;
			});
		}
		Insert<Ticket>(entity);
		return Save();
	}

	public Task Update(Ticket entity)
	{
		entity.CustomerId = _currentUserService.CustomerId.Value;
		Update(entity, true);
		return Save();
	}

	public Task Delete(Ticket entity)
	{
		Delete<Ticket>(entity);
		return Save();
	}
}
