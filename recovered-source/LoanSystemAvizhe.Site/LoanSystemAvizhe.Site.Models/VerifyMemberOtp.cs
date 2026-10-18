using System;

namespace LoanSystemAvizhe.Site.Models;

public class VerifyMemberOtp
{
    public long MemberId { get; set; }

    public Guid? MemberAuthorizationItemId { get; set; }

    public Guid? UserLoanRequestAuthorizationItemId { get; set; }

    public string Otp { get; set; }
}
