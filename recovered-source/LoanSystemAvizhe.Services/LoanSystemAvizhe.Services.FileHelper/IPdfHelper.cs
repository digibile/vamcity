namespace LoanSystemAvizhe.Services.FileHelper;

public interface IPdfHelper
{
    void ConvertHtmlToPdfAsync(string html, string outputPath);
}
