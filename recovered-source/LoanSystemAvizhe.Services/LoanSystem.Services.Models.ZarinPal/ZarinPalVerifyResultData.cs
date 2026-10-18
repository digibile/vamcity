namespace LoanSystem.Services.Models.ZarinPal;

public class ZarinPalVerifyResultData
{
    public int code { get; set; }

    public string message { get; set; }

    public string card_hash { get; set; }

    public string card_pan { get; set; }

    public long ref_id { get; set; }

    public string fee_type { get; set; }

    public int fee { get; set; }

    public string order_id { get; set; }
}
