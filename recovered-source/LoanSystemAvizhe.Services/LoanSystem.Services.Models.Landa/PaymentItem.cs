namespace LoanSystem.Services.Models.Landa;

public class PaymentItem
{
    public string cashId { get; set; }

    public string bankAccountId { get; set; }

    public decimal amount { get; set; }

    public int typeId { get; set; }
}
