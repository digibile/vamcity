using System.Drawing;
using System.Threading.Tasks;
using LoanSystemAvizhe.Services.Enums;
using Microsoft.AspNetCore.Http;

namespace LoanSystemAvizhe.Services.FileHelper;

public interface IFileHelper
{
	Task<string> UploadFile(IFormFile file, FileUploadDirectory fileUploadDirectory, string customerId = "", string fileName = "");

	bool CheckFileIsImage(IFormFile file);

	bool CheckFileNameIsImage(string fileName);

	bool CheckFileIsImageOrPdfOrRarOrZipOrXls(IFormFile file);

	void DeleteFile(string fileName, string directory);

	Image Base64ToImage(string base64);

	byte[] ReadFileAsByteArray(string relativePath);

	string ReadFileAsBase64(string relativePath);

	string GetUrl(FileUploadDirectory fileUploadDirectory, string imageFileName, string customerId);

	string ReturnDirectory(FileUploadDirectory fileUploadDirectory, string customerId);
}
