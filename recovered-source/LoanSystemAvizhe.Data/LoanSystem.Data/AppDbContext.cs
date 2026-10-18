using System.Data.Common;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoanSystem.Data;

public class AppDbContext : IdentityDbContext<User, Role, long, IdentityUserClaim<long>, IdentityUserRole<long>, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
{
    private readonly ICurrentUserService _currentUserService;

    private readonly ILoanRequestStatusChangeDbContextInterceptor _interceptor;

    public DbSet<Loan> Loans { get; set; }

    public DbSet<UserLoan> UserLoans { get; set; }

    public DbSet<AgentLoan> AgentLoans { get; set; }

    public DbSet<UserSavedMoney> UserSavedMonies { get; set; }

    public DbSet<UserLoanPayment> UserLoanPayments { get; set; }

    public DbSet<Expense> Expenses { get; set; }

    public DbSet<UserLoanRequest> UserLoanRequests { get; set; }

    public DbSet<UserLoanPaymentRequest> UserLoanPaymentRequests { get; set; }

    public DbSet<AppSettings> AppSettings { get; set; }

    public DbSet<Transaction> Withdraws { get; set; }

    public DbSet<SubAgentTransaction> SubAgentTransactions { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Purchase> Purchases { get; set; }

    public DbSet<MemberPurchase> MemberPurchases { get; set; }

    public DbSet<SmsPattern> SmsPatterns { get; set; }

    public DbSet<Consultant> Consultants { get; set; }

    public DbSet<AppInquiry> AppInquirys { get; set; }

    public DbSet<InquirySettings> InquirySettings { get; set; }

    public DbSet<AppSms> AppSms { get; set; }

    public DbSet<AppEmail> AppEmails { get; set; }

    public DbSet<Assurance> Assurances { get; set; }

    public DbSet<AffiliateCommission> AffiliateCommissions { get; set; }

    public DbSet<CustomerMarketerCommission> CustomerMarketerCommissions { get; set; }

    public DbSet<LoanAuthorizationItem> LoanAuthorizationItems { get; set; }

    public DbSet<BusinessAuthorizationItem> BusinessAuthorizationItems { get; set; }

    public DbSet<LoanContact> LoanContacts { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public DbSet<UserLoanRequestAuthorizationItemAttachment> UserLoanRequestAuthorizationItemAttachments { get; set; }

    public DbSet<UserLoanRequestAuthorizationItem> UserLoanRequestAuthorizationItems { get; set; }

    public DbSet<MemberAuthorizationItemAttachment> MemberAuthorizationItemAttachments { get; set; }

    public DbSet<MemberAuthorizationItem> MemberAuthorizationItems { get; set; }

    public DbSet<ThirdPersonAssuranceAuthorizationItem> ThirdPersonAssuranceAuthorizationItems { get; set; }

    public DbSet<EmzameContractAuthorizationItem> EmzameContractAuthorizationItems { get; set; }

    public DbSet<VisitorLog> VisitorLogs { get; set; }

    public DbSet<UserLoanRequestStatusHistory> UserLoanRequestStatusHistories { get; set; }

    public DbSet<LoanRequestStatus> LoanRequestStatuses { get; set; }

    public DbSet<Ticket> Tickets { get; set; }

    public DbSet<TicketResponse> TicketResponses { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<LoanTag> LoanTags { get; set; }

    public DbSet<CustomerOrder> CustomerOrders { get; set; }

    public DbSet<UserLoanPaymentOther> UserLoanPaymentOthers { get; set; }

    public DbSet<AgentFactor> AgentFactors { get; set; }

    public DbSet<AssuranceRequirement> AssuranceRequirements { get; set; }

    public DbSet<AssuranceRequirementItem> AssuranceRequirementItems { get; set; }

    public DbSet<LoanAssuranceRequirement> LoanAssuranceRequirements { get; set; }

    public DbSet<Reliability> Reliability { get; set; }

    public DbSet<SiteContent> SiteContents { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService, ILoanRequestStatusChangeDbContextInterceptor interceptor)
        : base((DbContextOptions)options)
    {
        _currentUserService = currentUserService;
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
    }

    public virtual DbCommand CreateCommand()
    {
        return Database.GetDbConnection().CreateCommand();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<User>().HasQueryFilter((User m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Loan>().HasQueryFilter((Loan m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Expense>().HasQueryFilter((Expense m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AppSettings>().HasQueryFilter((AppSettings m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Transaction>().HasQueryFilter((Transaction m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AppSms>().HasQueryFilter((AppSms m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AppInquiry>().HasQueryFilter((AppInquiry m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<LoanRequestStatus>().HasQueryFilter((LoanRequestStatus m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<UserLoanRequestStatusHistory>().HasQueryFilter((UserLoanRequestStatusHistory m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<ThirdPartySettings>().HasQueryFilter((ThirdPartySettings m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<ThirdPartyTransfer>().HasQueryFilter((ThirdPartyTransfer m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Ticket>().HasQueryFilter((Ticket m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<TicketResponse>().HasQueryFilter((TicketResponse m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Tag>().HasQueryFilter((Tag m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<CustomerOrder>().HasQueryFilter((CustomerOrder m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<UserRefahBeta>().HasQueryFilter((UserRefahBeta m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AgentFactor>().HasQueryFilter((AgentFactor m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Reliability>().HasQueryFilter((Reliability m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AssuranceRequirement>().HasQueryFilter((AssuranceRequirement m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<AssuranceRequirementItem>().HasQueryFilter((AssuranceRequirementItem m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<LoanAssuranceRequirement>().HasQueryFilter((LoanAssuranceRequirement m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<Reliability>().HasQueryFilter((Reliability m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<SiteContent>().HasQueryFilter((SiteContent m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<MemberAuthorizationItem>().HasQueryFilter((MemberAuthorizationItem m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<ThirdPersonAssuranceAuthorizationItem>().HasQueryFilter((ThirdPersonAssuranceAuthorizationItem m) => m.CustomerId == _currentUserService.CustomerId);
        modelBuilder.Entity<EmzameContractAuthorizationItem>().HasQueryFilter((EmzameContractAuthorizationItem m) => m.CustomerId == _currentUserService.CustomerId);
    }
}
