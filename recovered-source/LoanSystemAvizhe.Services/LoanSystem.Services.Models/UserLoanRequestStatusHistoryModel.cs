using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class UserLoanRequestStatusHistoryModel : BaseModel
{
    public User2Model User { get; set; }

    [Required(ErrorMessage = "شناسه درخواست وام الزامی است.")]
    public Guid UserLoanRequestId { get; set; }

    public DateTime DateTime { get; set; }

    public PersianDate DateTime_Persian => DateTime.ToPersianDateObject();

    public LoanRequestStatusModel FromStatus { get; }

    public LoanRequestStatusModel FromAgentStatus { get; }

    public LoanRequestStatusModel ToStatus { get; }

    public LoanRequestStatusModel ToAgentStatus { get; }

    public Guid CustomerId { get; set; }

    public UserLoanRequestStatusHistoryModel()
    {
    }

    public UserLoanRequestStatusHistoryModel(UserLoanRequestStatusHistory userLoanRequestStatusHistory)
    {
        Id = userLoanRequestStatusHistory.Id;
        UserLoanRequestId = userLoanRequestStatusHistory.UserLoanRequestId;
        DateTime = userLoanRequestStatusHistory.DateTime;
        if (userLoanRequestStatusHistory.User != null)
        {
            User = new User2Model(userLoanRequestStatusHistory.User);
        }
        if (userLoanRequestStatusHistory.FromStatus != null)
        {
            FromStatus = new LoanRequestStatusModel(userLoanRequestStatusHistory.FromStatus);
        }
        if (userLoanRequestStatusHistory.FromAgentStatus != null)
        {
            FromAgentStatus = new LoanRequestStatusModel(userLoanRequestStatusHistory.FromAgentStatus);
        }
        if (userLoanRequestStatusHistory.ToStatus != null)
        {
            ToStatus = new LoanRequestStatusModel(userLoanRequestStatusHistory.ToStatus);
        }
        if (userLoanRequestStatusHistory.ToAgentStatus != null)
        {
            ToAgentStatus = new LoanRequestStatusModel(userLoanRequestStatusHistory.ToAgentStatus);
        }
        CustomerId = userLoanRequestStatusHistory.CustomerId;
    }
}
