using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class TicketResponseModel : BaseModel
{
    public DateTime CreatedAt { get; set; }

    public PersianDate CreatedAt_Persian => CreatedAt.ToPersianDateObject();

    public IFormFile File { get; set; }

    [Required(ErrorMessage = "شرح پیام الزامی است.")]
    public string Description { get; set; }

    public string FileName { get; set; }

    public string FileUrl { get; set; }

    public string Base64 { get; set; }

    public bool FromAdmin { get; set; }

    public TicketRoles Role { get; set; }

    public string Role_Str => Role.ToString().Replace("_", " ");

    public bool IsSeen { get; set; }

    public Guid TicketId { get; set; }

    public string CreatedBy { get; set; }

    public TicketResponseModel()
    {
    }

    public TicketResponseModel(TicketResponse ticketResponse)
    {
        Id = ticketResponse.Id;
        CreatedAt = ticketResponse.CreatedAt;
        Description = ticketResponse.Description;
        FileName = ticketResponse.FileName;
        FromAdmin = ticketResponse.FromAdmin;
        IsSeen = ticketResponse.IsSeen;
        Role = ticketResponse.Role;
        TicketId = ticketResponse.TicketId;
        if (ticketResponse.CreatedBy != null)
        {
            CreatedBy = ticketResponse.CreatedBy.FullName;
        }
    }

    public void FillFileProps(IFileHelper fileHelper, Guid customerId)
    {
        if (string.IsNullOrEmpty(FileName) || customerId == Guid.Empty)
        {
            return;
        }
        if (!fileHelper.CheckFileNameIsImage(FileName))
        {
            FileUrl = fileHelper.GetUrl(FileUploadDirectory.Ticket, FileName, customerId.ToString());
            return;
        }
        string relativePath = Path.Combine(Directories.Ticket(customerId.ToString()), FileName);
        string text = fileHelper.ReadFileAsBase64(relativePath);
        if (!string.IsNullOrEmpty(text))
        {
            Base64 = "data:image/jpeg;base64," + text;
        }
    }
}
