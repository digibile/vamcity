using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanContactModel : BaseModel
{
    [Required(ErrorMessage = "طرف حساب الزامی است.")]
    public ContactModel Contact { get; set; }

    [Required(ErrorMessage = "وام الزامی است.")]
    public Guid UserLoanId { get; set; }

    [Required(ErrorMessage = "سهم طرف حساب الزامی است")]
    [Range(0.0, double.MaxValue, ErrorMessage = "سهم نماینده نمی تواند کمتر از 0 باشد")]
    public decimal Share { get; set; }

    public DateTime DueDate { get; set; }

    public PersianDate DueDate_Persian => DueDate.ToPersianDateObject();

    public DateTime UserLoanStartDate { get; set; }

    public PersianDate UserLoanStartDate_Persian => UserLoanStartDate.ToPersianDateObject();

    public int UserLoanCode { get; set; }

    public string UserLoanTitle { get; set; }

    public decimal UserLoanAmount { get; set; }

    public Member2Model MemberModel { get; set; }

    public decimal SumSponsorPayments { get; set; }

    public Guid CustomerId { get; set; }

    public UserLoanContactModel()
    {
    }

    public UserLoanContactModel(UserLoanContact userLoanContact)
    {
        Id = userLoanContact.Id;
        Share = userLoanContact.Share;
        DueDate = userLoanContact.DueDate;
        CustomerId = userLoanContact.CustomerId;
        UserLoanId = userLoanContact.UserLoanId;
        if (userLoanContact.UserLoan != null)
        {
            UserLoanCode = userLoanContact.UserLoan.Code;
            UserLoanAmount = userLoanContact.UserLoan.Amount;
            UserLoanStartDate = userLoanContact.UserLoan.StartDate;
            UserLoanTitle = userLoanContact.UserLoan.Loan?.Title ?? "";
            if (userLoanContact.UserLoan.User != null)
            {
                MemberModel = new Member2Model(userLoanContact.UserLoan.User);
            }
        }
        if (userLoanContact.Contact != null)
        {
            Contact = new ContactModel(userLoanContact.Contact);
        }
    }
}
