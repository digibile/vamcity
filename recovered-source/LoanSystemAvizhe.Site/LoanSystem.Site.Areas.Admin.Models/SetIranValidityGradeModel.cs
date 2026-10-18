using System;
using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SetIranValidityGradeModel
{
    [Required(ErrorMessage = "شناسه گزارش الزامی است.")]
    public Guid ReportId { get; set; }

    [Required(ErrorMessage = "گرید الزامی است.")]
    public string Grade { get; set; }

    [Required(ErrorMessage = "گرید رقمی الزامی است.")]
    public int GradeNumber { get; set; }
}
