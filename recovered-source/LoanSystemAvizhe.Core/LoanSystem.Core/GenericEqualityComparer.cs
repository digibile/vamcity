using System;
using System.Collections.Generic;

namespace LoanSystem.Core;

public class GenericEqualityComparer<T> : IEqualityComparer<T>
{
	private Func<T, T, bool> comparer;

	public GenericEqualityComparer(Func<T, T, bool> comparer)
	{
		this.comparer = comparer;
	}

	public bool Equals(T a, T b)
	{
		return comparer(a, b);
	}

	public int GetHashCode(T a)
	{
		return a.GetHashCode();
	}
}
