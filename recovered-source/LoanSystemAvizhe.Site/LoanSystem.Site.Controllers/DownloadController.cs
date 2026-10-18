using System.Collections.Generic;
using System.IO;
using LoanSystem.Core.Configuration;
using LoanSystemAvizhe.Core;
using LoanSystemAvizhe.Services.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NikanBase.Infrastructure.Framework;

namespace LoanSystem.Site.Controllers;

[Route("[controller]")]
public class DownloadController : BaseController
{
    private readonly IFileHelper _fileHelper;

    private readonly ICurrentUserService _currentUserService;

    private readonly AppConfig _appConfig;

    public DownloadController(ILogger<DownloadController> logger, IFileHelper fileHelper, ICurrentUserService currentUserService)
    {
        _fileHelper = fileHelper;
        _currentUserService = currentUserService;
    }

    [HttpGet("[action]/{customerId}/{uploadDirectory}/{fileName}/{downloadName?}")]
    public IActionResult Download(string customerId, FileUploadDirectory uploadDirectory, string fileName, string downloadName)
    {
        string path = _fileHelper.ReturnDirectory(uploadDirectory, customerId);
        string text = Path.Combine(path, fileName);
        byte[] fileContents = _fileHelper.ReadFileAsByteArray(text);
        return File(fileContents, GetContentType(text), (!string.IsNullOrEmpty(downloadName)) ? downloadName : fileName);
    }

    private string GetContentType(string path)
    {
        Dictionary<string, string> mimeTypes = GetMimeTypes();
        string key = Path.GetExtension(path).ToLowerInvariant();
        return mimeTypes[key];
    }

    private Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/vnd.ms-word" },
            { ".docx", "application/vnd.ms-word" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".csv", "text/csv" },
            { ".zip", "application/zip" },
            { ".rar", "application/vnd.rar" }
        };
    }
}
