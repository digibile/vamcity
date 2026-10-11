using System;
using System.Collections.Generic;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class TicketModel : BaseModel
{
	public int Code { get; set; }

	public DateTime CreatedAt { get; set; }

	public PersianDate CreatedAt_Persian => CreatedAt.ToPersianDateObject();

	public DateTime LastModifiedAt { get; set; }

	public PersianDate LastModifiedAt_Persian => LastModifiedAt.ToPersianDateObject();

	public string Title { get; set; }

	public TicketStatus Status { get; set; }

	public string Status_Str => Status.ToString().Replace("_", " ");

	public TicketRoles Role { get; set; }

	public string Role_Str => Role.ToString().Replace("_", " ");

	public TicketRoles TargetRole { get; set; }

	public string TargetRole_Str => TargetRole.ToString().Replace("_", " ");

	public int ResponseCount { get; set; }

	public bool AsMessage { get; set; }

	public Guid? UserLoanId { get; set; }

	public int? UserLoanCode { get; set; }

	public Guid? UserLoanRequestId { get; set; }

	public int? UserLoanRequestCode { get; set; }

	public string CreatedBy { get; set; }

	public long? TargetUserId { get; set; }

	public string TargetUserFullName { get; set; }

	public Guid? CustomerId { get; set; }

	public List<TicketResponseModel> TicketResponses { get; set; }

	public TicketModel()
	{
	}

	public TicketModel(Ticket ticket)
	{
		Id = ticket.Id;
		Code = ticket.Code;
		CreatedAt = ticket.CreatedAt;
		LastModifiedAt = ticket.LastModifiedAt;
		Title = ticket.Title;
		Status = ticket.Status;
		Role = ticket.Role;
		TargetRole = ticket.TargetRole;
		ResponseCount = ticket.ResponseCount;
		UserLoanId = ticket.UserLoanId;
		UserLoanRequestId = ticket.UserLoanRequestId;
		CustomerId = ticket.CustomerId;
		if (ticket.UserLoan != null)
		{
			UserLoanCode = ticket.UserLoan.Code;
		}
		if (ticket.UserLoanRequest != null)
		{
			UserLoanRequestCode = ticket.UserLoanRequest.Code;
		}
		if (ticket.CreatedBy != null)
		{
			CreatedBy = ticket.CreatedBy.FullName;
		}
		ICollection<TicketResponse> ticketResponses = ticket.TicketResponses;
		if (ticketResponses != null && ticketResponses.Count > 0)
		{
			TicketResponses = ticket.TicketResponses.Select((TicketResponse x) => new TicketResponseModel(x)).ToList();
		}
		if (ticket.TargetUser != null)
		{
			TargetUserId = ticket.TargetUserId;
			TargetUserFullName = ticket.TargetUser.FullName;
		}
	}
}
