using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;

namespace LoanSystem.Services.Models;

public class ConsultantModel : BaseModel
{
    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(100, ErrorMessage = "طول کاراکتر نام نباید بیش از 100 کاراکتر باشد.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "تلفن تماس الزامی است.")]
    [MaxLength(20, ErrorMessage = "طول کاراکتر شماره تماس نباید بیش از 20 کاراکتر باشد.")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "شرح الزامی است.")]
    [MaxLength(1000, ErrorMessage = "طول کاراکتر شرح نباید بیش از 1000 کاراکتر باشد.")]
    public string Description { get; set; }

    public bool IsSeen { get; set; }

    [MaxLength(1000, ErrorMessage = "طول کاراکتر نتیجه تماس نباید بیش از 1000 کاراکتر باشد.")]
    public string CallResult { get; set; }

    public DateTime SaveDate { get; set; }

    public PersianDate SaveDate_Persian => SaveDate.ToPersianDateObject();

    public ConsultantModel()
    {
    }

    public ConsultantModel(Consultant consultant)
    {
        Id = consultant.Id;
        Name = consultant.Name;
        Phone = consultant.Phone;
        Description = consultant.Description;
        IsSeen = consultant.IsSeen;
        CallResult = consultant.CallResult;
        SaveDate = consultant.SaveDate;
    }
}
