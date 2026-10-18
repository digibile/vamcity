using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.FileHelper;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class AssuranceModel : BaseModel
{
    public string Description { get; set; }

    public IFormFile File { get; set; }

    public string FileName { get; set; }

    public string Base64 { get; set; }

    public long FileSize { get; set; }

    public string FileType { get; set; }

    public IFormFile ReturnFile { get; set; }

    public string ReturnFileName { get; set; }

    public string ReturnBase64 { get; set; }

    public long ReturnFileSize { get; set; }

    public string ReturnFileType { get; set; }

    public string PersonFullName { get; set; }

    public string PersonNationalCode { get; set; }

    public string PersonalCode { get; set; }

    public string PersonMobile { get; set; }

    public bool MobileVerified { get; set; }

    public string MobileOtp { get; set; }

    public AssuranceType AssuranceType { get; set; }

    public bool IsReturned { get; set; }

    public DateTime ReturnDateTime { get; set; }

    public PersianDate ReturnDateTime_Persian => ReturnDateTime.ToPersianDateObject();

    public decimal GoldWeight { get; set; }

    public AssuranceCoinType CoinType { get; set; }

    public string ReturnedToPerson { get; set; }

    public Guid UserLoanId { get; set; }

    [Required(ErrorMessage = "وام الزامی است.")]
    public UserLoanModel UserLoanModel { get; set; }

    public OrganizationModel Organization { get; set; }

    public List<AssuranceChequeItemModel> AssuranceChequeItems { get; set; } = new List<AssuranceChequeItemModel>();

    public AssuranceModel()
    {
    }

    public AssuranceModel(Assurance assurance)
    {
        Id = assurance.Id;
        Description = assurance.Description;
        FileName = assurance.FileName;
        FileSize = assurance.FileSize;
        FileType = assurance.FileType;
        ReturnFileName = assurance.ReturnFileName;
        ReturnFileSize = assurance.ReturnFileSize;
        ReturnFileType = assurance.ReturnFileType;
        PersonFullName = assurance.PersonFullName;
        PersonNationalCode = assurance.PersonNationalCode;
        PersonalCode = assurance.PersonalCode;
        PersonMobile = assurance.PersonMobile;
        MobileVerified = assurance.MobileVerified;
        AssuranceType = assurance.AssuranceType;
        GoldWeight = assurance.GoldWeight;
        CoinType = assurance.CoinType;
        IsReturned = assurance.IsReturned;
        ReturnDateTime = assurance.ReturnDateTime;
        ReturnedToPerson = assurance.ReturnedToPerson;
        UserLoanId = assurance.UserLoanId;
        if (assurance.Organization != null)
        {
            Organization = new OrganizationModel(assurance.Organization);
        }
        if (assurance.UserLoan != null)
        {
            UserLoanModel = new UserLoanModel(assurance.UserLoan);
        }
        ICollection<AssuranceChequeItem> assuranceChequeItems = assurance.AssuranceChequeItems;
        if (assuranceChequeItems != null && assuranceChequeItems.Any())
        {
            AssuranceChequeItems = assurance.AssuranceChequeItems.Select((AssuranceChequeItem x) => new AssuranceChequeItemModel(x)).ToList();
        }
    }

    public void FillImageBase64Prop(IFileHelper fileHelper, Guid customerId)
    {
        if ((string.IsNullOrEmpty(FileName) && string.IsNullOrEmpty(ReturnFileName)) || customerId == Guid.Empty)
        {
            return;
        }
        if (!string.IsNullOrEmpty(FileName))
        {
            string relativePath = Path.Combine(Directories.Assurance(customerId.ToString()), FileName);
            string text = fileHelper.ReadFileAsBase64(relativePath);
            if (!string.IsNullOrEmpty(text))
            {
                Base64 = "data:image/jpeg;base64," + text;
            }
        }
        if (!string.IsNullOrEmpty(ReturnFileName))
        {
            string relativePath2 = Path.Combine(Directories.Assurance(customerId.ToString()), ReturnFileName);
            string text2 = fileHelper.ReadFileAsBase64(relativePath2);
            if (!string.IsNullOrEmpty(text2))
            {
                ReturnBase64 = "data:image/jpeg;base64," + text2;
            }
        }
    }
}
