namespace LoanSystem.Services.Models.Ice;

public class IceAddIdentificationFromFirmRequest
{
    public string NationalCode { get; set; }

    public string CellPhone { get; set; }

    public string CompanyNationalID { get; set; } = "";

    public UserPaymentTypeEnum UserPaymentTypeEnum { get; set; } = UserPaymentTypeEnum.ByUs;
}
