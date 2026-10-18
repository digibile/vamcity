using System;
using System.IO;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class MemberAuthorizationItemAttachmentModel : BaseModel
{
    public string RowState { get; set; } = "unchanged";

    public MemberAuthorizationItem MemberAuthorizationItem { get; set; }

    public string Attachment { get; set; }

    public string MemberDescription { get; set; } = "";

    public bool AttachmentByAdmin { get; set; }

    public Guid CustomerId { get; set; }

    public IFormFile File { get; set; }

    public string Base64 { get; set; }

    public string FileUrl { get; set; }

    public MemberAuthorizationItemAttachmentModel()
    {
    }

    public MemberAuthorizationItemAttachmentModel(MemberAuthorizationItemAttachment item)
    {
        Id = item.Id;
        Attachment = item.Attachment;
        MemberDescription = item.MemberDescription;
        AttachmentByAdmin = item.AttachmentByAdmin;
        CustomerId = item.CustomerId;
    }

    public void FillFileProps(IFileHelper fileHelper, Guid customerId)
    {
        if (string.IsNullOrEmpty(Attachment) || customerId == Guid.Empty)
        {
            return;
        }
        if (!fileHelper.CheckFileNameIsImage(Attachment))
        {
            FileUrl = fileHelper.GetUrl(FileUploadDirectory.AuthorizeItem, Attachment, customerId.ToString());
            return;
        }
        string relativePath = Path.Combine(Directories.AuthorizeItem(customerId.ToString()), Attachment);
        string text = fileHelper.ReadFileAsBase64(relativePath);
        if (!string.IsNullOrEmpty(text))
        {
            Base64 = "data:image/jpeg;base64," + text;
        }
    }
}
