using System;
using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Site.Areas.Admin.Models;

public class SetIceGradeModel
{
    [Required(ErrorMessage = "شناسه آیس الزامی است.")]
    public Guid IceReportId { get; set; }

    [Required(ErrorMessage = "گرید الزامی است.")]
    public string Grade { get; set; }
}
