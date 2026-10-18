namespace LoanSystemAvizhe.Services.Models.Shahin;

public class JsonObj_transfer_validation : JsonObj
{
    public string oneWithdraw { get; set; }

    public string destinationBank { get; set; }

    public string correlationID { get; set; }

    public string destinationAccountNumber { get; set; }

    public string amount { get; set; }

    public string babat { get; set; }

    public string documentID { get; set; }

    public string paymentID { get; set; }

    public string transferType { get; set; }

    public string description { get; set; }

    public string toNationalID { get; set; }

    public string toPostalID { get; set; }
}
