using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.Models.Emzame;

namespace LoanSystem.Services.Models;

public class EmzameContractAuthorizationItemModel : BaseModel
{
    public SendEmzameContractResult Contract { get; set; }

    public Guid? UserLoanRequestAuthorizationItemId { get; set; }

    public UserLoanRequestAuthorizationItemModel UserLoanRequestAuthorizationItem { get; set; }

    public Guid? UserLoanId { get; set; }

    public UserLoanModel UserLoan { get; set; }

    public Member3Model User { get; set; }

    [Required(ErrorMessage = "شناسه مدرک الزامی است.")]
    public Guid AuthorizationItemId { get; set; }

    public string Comment { get; set; }

    public string Tag { get; set; }

    public Guid CustomerId { get; set; }

    public EmzameContractAuthorizationItemModel()
    {
    }

    public EmzameContractAuthorizationItemModel(EmzameContractAuthorizationItem item)
    {
        Id = item.Id;
        UserLoanRequestAuthorizationItemId = item.UserLoanRequestAuthorizationItemId;
        AuthorizationItemId = item.AuthorizationItemId;
        if (item.User != null)
        {
            User = new Member3Model(item.User);
        }
        if (!string.IsNullOrWhiteSpace(item.ContractJson))
        {
            Contract = JsonSerializer.Deserialize<SendEmzameContractResult>(item.ContractJson);
        }
        Tag = item.Tag;
        Comment = item.Comment;
        CustomerId = item.CustomerId;
    }

    public void SetUserLoanRequestAuthorizationItem(UserLoanRequestAuthorizationItemModel model)
    {
        UserLoanRequestAuthorizationItem = model;
    }
}
