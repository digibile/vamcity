using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystemAvizhe.Services.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;

namespace LoanSystemAvizhe.Services.FileHelper;

public class FileHelper : IFileHelper
{
    private static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

    private readonly IHostingEnvironment _appEnvironment;

    public FileHelper(IHostingEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public async Task<string> UploadFile(IFormFile file, FileUploadDirectory fileUploadDirectory, string customerId = "", string fileName = "")
    {
        if (file != null)
        {
            string text = ReturnDirectory(fileUploadDirectory, customerId);
            if (text == null)
            {
                return null;
            }
            string text2 = Path.Combine(_appEnvironment.ContentRootPath, text);
            if (!Directory.Exists(text2))
            {
                Directory.CreateDirectory(text2);
            }
            if (file.Length > 0)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    fileName = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                }
                using FileStream fileStream = new FileStream(Path.Combine(text2, fileName), FileMode.Create);
                await file.CopyToAsync(fileStream);
            }
        }
        return fileName;
    }

    public string ReadFileAsBase64(string relativePath)
    {
        string text = Path.Combine(_appEnvironment.ContentRootPath, relativePath);
        if (!File.Exists(text))
        {
            return null;
        }
        using Image image = Image.FromFile(text);
        using MemoryStream memoryStream = new MemoryStream();
        image.Save(memoryStream, image.RawFormat);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public byte[] ReadFileAsByteArray(string relativePath)
    {
        string path = Path.Combine(_appEnvironment.ContentRootPath, relativePath);
        if (!File.Exists(path))
        {
            return null;
        }
        return File.ReadAllBytes(path);
    }

    public bool CheckFileIsImage(IFormFile file)
    {
        try
        {
            Base64ToImage(Convert.ToBase64String(GetFileByte(file)));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckFileIsPdf(IFormFile file)
    {
        try
        {
            bool flag = false;
            bool flag2 = false;
            if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                flag = true;
            }
            if (file.Length >= 4)
            {
                using Stream input = file.OpenReadStream();
                using BinaryReader binaryReader = new BinaryReader(input);
                if (Enumerable.SequenceEqual(binaryReader.ReadBytes(4), new byte[4] { 37, 80, 68, 70 }))
                {
                    flag2 = true;
                }
            }
            return flag & flag2;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckFileIsRarOrZip(IFormFile file)
    {
        try
        {
            bool flag = false;
            bool flag2 = ZipArchive.IsZipFile(file.OpenReadStream());
            flag = RarArchive.IsRarFile(file.OpenReadStream());
            return flag2 | flag;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckFileIsImageOrPdfOrRarOrZipOrXls(IFormFile file)
    {
        try
        {
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = CheckFileIsImage(file);
            flag = CheckFileIsPdf(file);
            flag2 = CheckFileIsRarOrZip(file);
            flag3 = CheckFileIsExcel(file);
            return flag4 | flag | flag2 | flag3;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool CheckFileIsExcel(IFormFile file)
    {
        using (Stream stream = file.OpenReadStream())
        {
            byte[] array = new byte[8];
            stream.Read(array, 0, 8);
            if (array[0] == 208 && array[1] == 207 && array[2] == 17 && array[3] == 224 && array[4] == 161 && array[5] == 177 && array[6] == 26 && array[7] == 225)
            {
                return true;
            }
            if (array[0] == 80 && array[1] == 75 && array[2] == 3 && array[3] == 4)
            {
                return true;
            }
        }
        return false;
    }

    public void DeleteFile(string fileName, string directory)
    {
        if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(directory))
        {
            string path = Path.Combine(_appEnvironment.ContentRootPath, directory, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public Image Base64ToImage(string base64)
    {
        using MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64));
        return Image.FromStream(stream);
    }

    public static byte[] GetFileByte(IFormFile formFile)
    {
        using MemoryStream memoryStream = new MemoryStream();
        formFile.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public bool CheckFileNameIsImage(string fileName)
    {
        List<string> list = new List<string> { ".jpg", ".jpeg", ".png", ".Jpg", ".Jpeg", ".Png", ".Bmp", ".Tiff", ".jfif" };
        string extension = Path.GetExtension(fileName);
        return list.Contains(extension);
    }

    public string GetUrl(FileUploadDirectory fileUploadDirectory, string fileName, string customerId)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "";
        }
        return $"/Download/Download/{customerId}/{(short)fileUploadDirectory}/{fileName}";
    }

    public string ReturnDirectory(FileUploadDirectory fileUploadDirectory, string customerId)
    {
        return fileUploadDirectory switch
        {
            FileUploadDirectory.Ticket => Directories.Ticket(customerId),
            FileUploadDirectory.Assurance => Directories.Assurance(customerId),
            FileUploadDirectory.Loan => Directories.Loan(customerId),
            FileUploadDirectory.AuthorizeItem => Directories.AuthorizeItem(customerId),
            FileUploadDirectory.Transaction => Directories.Transaction(customerId),
            FileUploadDirectory.UserLoanPaymentRequest => Directories.UserLoanPaymentRequest(customerId),
            FileUploadDirectory.AgentAttachment => Directories.AgentAttachments(customerId),
            FileUploadDirectory.AssuranceCheque => Directories.AssuranceCheque(customerId),
            FileUploadDirectory.SiteContent => Directories.SiteContent(customerId),
            _ => null,
        };
    }
}
