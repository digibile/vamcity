using System;
using System.Collections.Generic;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class Assurance : BaseEntity
{
    public string Description { get; set; }

    public string FileName { get; set; }

    public long FileSize { get; set; }

    public string FileType { get; set; }

    public string ReturnFileName { get; set; }

    public long ReturnFileSize { get; set; }

    public string ReturnFileType { get; set; }

    public string PersonFullName { get; set; }

    public string PersonNationalCode { get; set; }

    public string PersonalCode { get; set; }

    public string PersonMobile { get; set; }

    public bool MobileVerified { get; set; }

    public AssuranceType AssuranceType { get; set; }

    public bool IsReturned { get; set; }

    public DateTime ReturnDateTime { get; set; }

    public string ReturnedToPerson { get; set; }

    public decimal GoldWeight { get; set; }

    public AssuranceCoinType CoinType { get; set; }

    public Guid UserLoanId { get; set; }

    public virtual UserLoan UserLoan { get; set; }

    public Guid? OrganizationId { get; set; }

    public virtual Organization Organization { get; set; }

    public virtual ICollection<AssuranceChequeItem> AssuranceChequeItems { get; set; } = new HashSet<AssuranceChequeItem>();
}
