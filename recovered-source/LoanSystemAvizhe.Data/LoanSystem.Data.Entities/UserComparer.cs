using System.Collections.Generic;

namespace LoanSystem.Data.Entities;

public class UserComparer : IEqualityComparer<User>
{
	public bool Equals(User left, User right)
	{
		if (left == null && right == null)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		return left.Id == right.Id;
	}

	public int GetHashCode(User user)
	{
		return (user.Id + user.UserName).GetHashCode();
	}
}
