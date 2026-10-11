using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LoanSystemAvizhe.Core;

public interface ILoanRequestStatusChangeDbContextInterceptor : ISaveChangesInterceptor, IInterceptor
{
}
