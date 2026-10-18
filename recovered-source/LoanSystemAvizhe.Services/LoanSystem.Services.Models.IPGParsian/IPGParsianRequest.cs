namespace LoanSystem.Services.Models.IPGParsian;

public class IPGParsianRequest
{
    public string AdditionalData { get; set; }

    public long? Amount { get; set; }

    public string CallBackUrl { get; set; }

    public string LoginAccount { get; set; }

    public long? OrderId { get; set; }

    public string Originator { get; set; }
}
