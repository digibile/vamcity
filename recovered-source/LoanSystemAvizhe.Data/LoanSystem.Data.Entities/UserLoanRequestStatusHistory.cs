using System;

namespace LoanSystem.Data.Entities;

public class UserLoanRequestStatusHistory : BaseEntity
{
    public long UserId { get; set; }

    public virtual User User { get; set; }

    public Guid UserLoanRequestId { get; set; }

    public virtual UserLoanRequest UserLoanRequest { get; set; }

    public DateTime DateTime { get; set; }

    public Guid? FromStatusId { get; set; }

    public virtual LoanRequestStatus FromStatus { get; }

    public Guid? FromAgentStatusId { get; set; }

    public virtual LoanRequestStatus FromAgentStatus { get; }

    public Guid? ToStatusId { get; set; }

    public virtual LoanRequestStatus ToStatus { get; }

    public Guid? ToAgentStatusId { get; set; }

    public virtual LoanRequestStatus ToAgentStatus { get; }

    public Guid CustomerId { get; set; }
}
