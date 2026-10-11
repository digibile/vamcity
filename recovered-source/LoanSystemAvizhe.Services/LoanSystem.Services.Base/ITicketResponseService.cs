using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Base;

public interface ITicketResponseService
{
	Task<List<TicketResponse>> GetAllTicketResponses();

	Task<List<TicketResponse>> GetByTicketId(Guid ticketId, params string[] includs);

	Task<TicketResponse> GetById(Guid entityId, params string[] includs);

	Task Insert(TicketResponse entity);

	Task Update(TicketResponse entity);

	Task Delete(TicketResponse entity);
}
