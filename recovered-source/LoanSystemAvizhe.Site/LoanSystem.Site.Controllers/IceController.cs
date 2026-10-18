using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystemAvizhe.Services.FileHelper;
using LoanSystemAvizhe.Services.ThirdParty;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoanSystem.Site.Controllers;

[ApiController]
[Route("[controller]")]
public class IceController : BaseDefaultController
{
    private readonly ICustomerService _customerService;

    private readonly IIceReportService _iceReportService;

    private readonly IHostingEnvironment _hostingEnvironment;

    public IceController(ILogger<SiteController> logger, ICustomerService customerService, IIceReportService iceReportService, IHostingEnvironment hostingEnvironment)
        : base(logger)
    {
        _customerService = customerService;
        _iceReportService = iceReportService;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet("[action]/{customerCode}")]
    public async Task<IActionResult> ReportReady(int customerCode)
    {
        Customer customer = await _customerService.GetByCode(customerCode);
        if (customer == null)
        {
            return Ok();
        }
        if (!Request.Query.TryGetValue("ReportId", out var reportId))
        {
            return Ok();
        }
        Request.Query.TryGetValue("ReportLink", out var reportLink);
        Request.Query.TryGetValue("ReportPdf", out var reportPdf);
        IceReport iceReport = (await _iceReportService.GetByCondition((IceReport x) => x.ReportID == ((object)reportId).ToString())).FirstOrDefault();
        if (iceReport == null)
        {
            return Ok();
        }
        if (!string.IsNullOrEmpty(reportPdf))
        {
            try
            {
                WebClient webClient = new WebClient();
                string text = Directories.IceReportPdf(customer.Id.ToString());
                if (text == null)
                {
                    throw new Exception();
                }
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                string text2 = Path.Combine(contentRootPath, text);
                if (!Directory.Exists(text2))
                {
                    Directory.CreateDirectory(text2);
                }
                string text3 = Guid.NewGuid().ToString().Replace("-", "") + ".pdf";
                string fileName = Path.Combine(text2, text3);
                webClient.DownloadFile(new Uri(reportPdf), fileName);
                iceReport.ReportPdfFileName = text3;
            }
            catch
            {
            }
        }
        iceReport.Url = reportLink.ToString();
        iceReport.Status = "گزارش آماده شده";
        await _iceReportService.Update(iceReport);
        return Ok("");
    }

    [HttpGet("[action]/{iceReportId}")]
    public async Task<IActionResult> DownloadPdf(Guid iceReportId)
    {
        IceReport iceReport = await _iceReportService.AsQueryable().Include((IceReport x) => x.Member).IgnoreQueryFilters()
            .FirstOrDefaultAsync((IceReport x) => x.Id == iceReportId);
        if (iceReport == null || string.IsNullOrEmpty(iceReport.ReportPdfFileName))
        {
            return NotFound();
        }
        string path = Directories.IceReportPdf(iceReport.CustomerId.ToString());
        string contentRootPath = _hostingEnvironment.ContentRootPath;
        string path2 = Path.Combine(contentRootPath, path, iceReport.ReportPdfFileName);
        if (!System.IO.File.Exists(path2))
        {
            return NotFound();
        }
        byte[] fileContents = System.IO.File.ReadAllBytes(path2);
        return File(fileContents, "application/pdf", "گزارش اعتبارسنجی " + iceReport.Member?.FullName + ".pdf");
    }
}
