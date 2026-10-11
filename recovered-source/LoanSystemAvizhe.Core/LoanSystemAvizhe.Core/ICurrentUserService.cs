using System;

namespace LoanSystemAvizhe.Core;

public interface ICurrentUserService
{
	long? UserId { get; set; }

	Guid? CustomerId { get; set; }

	bool? IsOwner { get; }

	bool IsAuthenticated { get; }

	void EnsureHasCustomer()
	{
		if (!CustomerId.HasValue)
		{
			throw new UnauthorizedAccessException();
		}
	}

	void EnsureAuthenticated()
	{
		if (!IsAuthenticated)
		{
			throw new UnauthorizedAccessException();
		}
	}

	bool IsInRole(string role);

	bool TrySetCustomerIdFromToken();
}
