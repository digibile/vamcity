using System;
using System.ComponentModel.DataAnnotations;
using LoanSystem.Core;
using LoanSystem.Data.Entities;
using LoanSystemAvizhe.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace LoanSystem.Services.Models;

public class AgentMetaDataModel : BaseModel
{
    [RequiredIf("AgentType", AgentType.Hoghoghi, "نام صاحبین امضا الزامی است.")]
    public string SignaturesOwnerNames { get; set; }

    [RequiredIf("AgentType", AgentType.Hoghoghi, "شماره ثبت شرکت الزامی است.")]
    public string ShomareSabt { get; set; }

    [RequiredIf("AgentType", AgentType.Hoghoghi, "شناسه ملی الزامی است.")]
    public string ShenaseMelli { get; set; }

    [RequiredIf("AgentType", AgentType.Hoghoghi, "نام شرکت الزامی است.")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "تعیین نوع محصولات کسب و کار الزامی است")]
    public string AgentCategoryActivities { get; set; }

    public AgentType AgentType { get; set; }

    public Guid CustomerId { get; set; }

    public AgentStatus Status { get; set; }

    [MaxLength(100, ErrorMessage = "حداکثر طول آدرس وب سایت 100 کاراکتر باید باشد.")]
    public string SiteUrl { get; set; }

    public bool IsOnline { get; set; }

    public bool IsPhysical { get; set; }

    public string RejectReason { get; set; }

    public DateTime LastAdminChangeDateTime { get; set; }

    public PersianDate LastAdminChangeDateTime_Persian => LastAdminChangeDateTime.ToPersianDateObject();

    public string CheckedBy { get; set; }

    public IFormFile ThumbnailFile { get; set; }

    public string ThumbnailFileName { get; set; }

    public string ThumbnailBase64 { get; set; }

    public IFormFile JavazKasbFile { get; set; }

    public string JavazKasbFileName { get; set; }

    public string JavazKasbBase64 { get; set; }

    public IFormFile KartMelliFile { get; set; }

    public string KartMelliFileName { get; set; }

    public string KartMelliBase64 { get; set; }

    public IFormFile EjarenameFile { get; set; }

    public string EjarenameFileName { get; set; }

    public string EjarenameBase64 { get; set; }

    public IFormFile AsasnameFile { get; set; }

    public string AsasnameFileName { get; set; }

    public string AsasnameBase64 { get; set; }

    public IFormFile RoznameRasmiFile { get; set; }

    public string RoznameRasmiFileName { get; set; }

    public string RoznameRasmiBase64 { get; set; }

    public IFormFile PledgeChequeFile { get; set; }

    public string PledgeChequeFileName { get; set; }

    public string PledgeChequeBase64 { get; set; }

    public bool UseDefaultCommission { get; set; }

    public decimal AgentFactorCommission { get; set; }

    public decimal MaxAgentFactorCommission { get; set; }

    public AgentMetaDataModel()
    {
    }

    public AgentMetaDataModel(AgentMetaData agentMetaData)
    {
        Id = agentMetaData.Id;
        SignaturesOwnerNames = agentMetaData.SignaturesOwnerNames;
        ShomareSabt = agentMetaData.ShomareSabt;
        ShenaseMelli = agentMetaData.ShenaseMelli;
        CompanyName = agentMetaData.CompanyName;
        SiteUrl = agentMetaData.SiteUrl;
        IsOnline = agentMetaData.IsOnline;
        IsPhysical = agentMetaData.IsPhysical;
        AgentCategoryActivities = agentMetaData.AgentCategoryActivities;
        AgentType = agentMetaData.AgentType;
        CustomerId = agentMetaData.CustomerId;
        Status = agentMetaData.Status;
        RejectReason = agentMetaData.RejectReason;
        LastAdminChangeDateTime = agentMetaData.LastAdminChangeDateTime;
        CheckedBy = agentMetaData.CheckedBy;
        KartMelliFileName = agentMetaData.KartMelliFileName;
        EjarenameFileName = agentMetaData.EjarenameFileName;
        AsasnameFileName = agentMetaData.AsasnameFileName;
        RoznameRasmiFileName = agentMetaData.RoznameRasmiFileName;
        JavazKasbFileName = agentMetaData.JavazKasbFileName;
        ThumbnailFileName = agentMetaData.ThumbnailFileName;
        PledgeChequeFileName = agentMetaData.PledgeChequeFileName;
        UseDefaultCommission = agentMetaData.UseDefaultCommission;
        AgentFactorCommission = agentMetaData.AgentFactorCommission;
        MaxAgentFactorCommission = agentMetaData.MaxAgentFactorCommission;
    }
}
