namespace LoanSystemAvizhe.Services.Models.Shahin;

public class JsonObj_transfer : JsonObj
{
    public string destinationAccountNumber { get; set; }

    public string destinationAccountName { get; set; }

    public string destinationBank { get; set; }

    public string amount { get; set; }

    public string transferType { get; set; }

    public string babat { get; set; }

    public string paymentID { get; set; }

    public string documentID { get; set; }

    public string transferID { get; set; }

    public string depositDescription { get; set; }

    public string withdrawDescription { get; set; }

    public string smsPass { get; set; }
}
