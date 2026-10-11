using System;

namespace LoanSystem.Data.Entities;

public class VisitorLog : BaseEntity
{
	public DateTime Date { get; set; }

	public string IpAddress { get; set; }

	public string UserAgent { get; set; }

	public string Mrk { get; set; }
}
