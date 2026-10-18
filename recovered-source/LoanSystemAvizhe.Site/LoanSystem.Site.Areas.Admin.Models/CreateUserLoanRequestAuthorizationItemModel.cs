using System;
using System.ComponentModel.DataAnnotations;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Site.Areas.Admin.Models;

public class CreateUserLoanRequestAuthorizationItemModel
{
    [Required(ErrorMessage = "نوع سیستمی مدرک الزامی است.")]
    public AuthorizationItemSystemType SystemType { get; set; }

    [Required(ErrorMessage = "شناسه مشتری الزامی است.")]
    public long MemberId { get; set; }

    [Required(ErrorMessage = "شناسه مدرک الزامی است.")]
    public Guid AuthorizationItemId { get; set; }

    [Required(ErrorMessage = "شناسه درخواست وام الزامی است.")]
    public Guid UserLoanRequestId { get; set; }
}
