using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Services.Models;

public class ReliabilityModel : BaseModel
{
	[Required(ErrorMessage = "عنوان الزامی است.")]
	public string Title { get; set; }

	[Required(ErrorMessage = "رنگ الزامی است.")]
	public string Color { get; set; }

	public int From { get; set; }

	public int To { get; set; }

	public int Count { get; set; }

	public ReliabilityRowTypes RowType { get; set; }

	public bool IsNewLoanLock { get; set; }

	public Guid CustomerId { get; set; }

	public ReliabilityModel()
	{
	}

	public ReliabilityModel(Reliability reliability)
	{
		Id = reliability.Id;
		Title = reliability.Title;
		Color = reliability.Color;
		From = reliability.From;
		To = reliability.To;
		Count = reliability.Count;
		RowType = reliability.RowType;
		IsNewLoanLock = reliability.IsNewLoanLock;
		CustomerId = reliability.CustomerId;
	}
}
