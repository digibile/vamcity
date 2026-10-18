using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LoanSystem.Data.Entities;

namespace LoanSystemAvizhe.Site.Helpers;

public class LoanRequestStatusComparer : IEqualityComparer<LoanRequestStatus>
{
    public bool Equals(LoanRequestStatus x, LoanRequestStatus y)
    {
        return x.Id == y.Id;
    }

    public int GetHashCode([DisallowNull] LoanRequestStatus obj)
    {
        return obj.Id.GetHashCode();
    }
}
