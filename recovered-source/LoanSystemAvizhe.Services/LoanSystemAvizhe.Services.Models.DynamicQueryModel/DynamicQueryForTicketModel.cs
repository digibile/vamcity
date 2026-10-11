using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystemAvizhe.Services.Models.DynamicQueryModel;

public class DynamicQueryForTicketModel
{
	public TicketStatus? StatusFilter { get; set; }

	public long? MemberId { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public List<string> SortBy { get; set; }

	public List<bool> SortDesc { get; set; }

	public int Page { get; set; }

	public int ItemsPerPage { get; set; }

	public string Search { get; set; }
}
