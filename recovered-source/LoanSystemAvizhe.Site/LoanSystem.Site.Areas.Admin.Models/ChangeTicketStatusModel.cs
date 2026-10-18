using System;
using System.ComponentModel.DataAnnotations;
using LoanSystemAvizhe.Data.Enums;

namespace LoanSystem.Site.Areas.Admin.Models;

public class ChangeTicketStatusModel
{
    public TicketStatus Status { get; set; }

    [Required(ErrorMessage = "شناسه تیکت الزامی است.")]
    public Guid TicketId { get; set; }
}
