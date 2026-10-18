using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace LoanSystem.Data.Entities;

public class User : IdentityUser<long>
{
    public string UserCode { get; set; }

    public Guid? AgentMetaDataId { get; set; }

    public virtual AgentMetaData AgentMetaData { get; set; }

    public string AffiliateCode { get; set; }

    public int CommissionPercent { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public bool Gender { get; set; }

    public string Address { get; set; }

    public string Mobile { get; set; }

    public decimal Credit { get; set; }

    public bool WalletIsLock { get; set; }

    public string Phone { get; set; }

    public DateTime LastLoginDate { get; set; }

    public DateTime LastLoginDate2 { get; set; }

    public bool IsActive { get; set; }

    [NotMapped]
    public List<string> UserRoleNames { get; set; }

    public string NationalCode { get; set; }

    public decimal TotalSavedMoney { get; set; }

    public int ViewCount { get; set; }

    public DateTime RegDate { get; set; }

    public DateTime RegDate2 { get; set; }

    public string FullName => FirstName + " " + LastName;

    public DateTime? BirthDate { get; set; }

    public string FatherName { get; set; }

    public string Shenasname { get; set; }

    public string City { get; set; }

    public string Familiar1FirstName { get; set; }

    public string Familiar1LastName { get; set; }

    public string Familiar1Mobile { get; set; }

    public string Familiar2FirstName { get; set; }

    public string Familiar2LastName { get; set; }

    public string Familiar2Mobile { get; set; }

    public bool RegistrationCompleted { get; set; }

    public bool MobileVerified { get; set; }

    public string PostalCode { get; set; }

    public int Otp { get; set; }

    public string BusinessName { get; set; }

    public string BankAccountsJson { get; set; }

    public long? AgentId { get; set; }

    public bool SmsActiveForMembers { get; set; }

    public virtual User Agent { get; set; }

    public long? CustomerMarketerId { get; set; }

    public virtual User CustomerMarketer { get; set; }

    public Guid? CustomerId { get; set; }

    public virtual Reliability Reliability { get; set; }

    public Guid? ReliabilityId { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<AgentLoan> AgentLoans { get; set; } = new HashSet<AgentLoan>();

    public User()
    {
        UserRoleNames = new List<string>();
    }
}
