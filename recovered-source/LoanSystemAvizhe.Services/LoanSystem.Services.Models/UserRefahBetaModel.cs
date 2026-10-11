using System;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserRefahBetaModel : BaseModel
{
	public DateTime ModifiedAt { get; set; }

	public string Message { get; set; }

	public long Amount { get; set; }

	public bool IsSuccess { get; set; }

	public Member2Model User { get; set; }

	public UserRefahBetaModel()
	{
	}

	public UserRefahBetaModel(UserRefahBeta userRefahBeta)
	{
		Id = userRefahBeta.Id;
		ModifiedAt = userRefahBeta.ModifiedAt;
		Message = userRefahBeta.Message;
		Amount = userRefahBeta.Amount;
		IsSuccess = userRefahBeta.IsSuccess;
		if (userRefahBeta.User != null)
		{
			User = new Member2Model(userRefahBeta.User);
		}
	}
}
