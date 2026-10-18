namespace LoanSystem.Services.Models.NextPay;

public class NextPayVerifyResult
{
    public int code { get; set; }

    public int amount { get; set; }

    public string order_id { get; set; }

    public string customer_phone { get; set; }

    public string Shaparak_Ref_Id { get; set; }
}
