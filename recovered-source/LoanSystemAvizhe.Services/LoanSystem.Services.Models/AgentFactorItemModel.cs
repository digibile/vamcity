using System;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class AgentFactorItemModel : BaseModel
{
	public Guid AgentFactorId { get; set; }

	public string Product { get; set; }

	public decimal Payable { get; set; }

	public decimal Count { get; set; }

	public decimal Fee { get; set; }

	public Guid CustomerId { get; set; }

	public AgentFactorItemModel()
	{
	}

	public AgentFactorItemModel(AgentFactorItem agentFactorItem)
	{
		Id = agentFactorItem.Id;
		Product = agentFactorItem.Product;
		Payable = agentFactorItem.Payable;
		Count = agentFactorItem.Count;
		Fee = agentFactorItem.Fee;
		CustomerId = agentFactorItem.CustomerId;
	}
}
