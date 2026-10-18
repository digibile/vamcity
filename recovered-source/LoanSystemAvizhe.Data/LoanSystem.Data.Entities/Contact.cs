using System;
using System.Collections.Generic;

namespace LoanSystem.Data.Entities;

public class Contact : BaseEntity
{
    public string FullName { get; set; }

    public string BusinessName { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public string BankInfo { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<UserLoanContact> UserLoanContacts { get; set; } = new HashSet<UserLoanContact>();
}
