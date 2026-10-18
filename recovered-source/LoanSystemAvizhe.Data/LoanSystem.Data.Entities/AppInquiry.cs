using System;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Data.Entities;

public class AppInquiry : BaseEntity
{
    public DateTime Date { get; set; }

    public InquiryPanel InquiryPanel { get; set; }

    public AppInquiryType AppInquiryType { get; set; }

    public decimal Price { get; set; }

    public decimal MemberPrice { get; set; }

    public bool IsSuccess { get; set; }

    public string PanelResultJson { get; set; }

    public string Result { get; set; }

    public string Key { get; set; }

    public string Tag { get; set; }

    public long? MemberId { get; set; }

    public virtual User Member { get; set; }

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
