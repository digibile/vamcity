namespace LoanSystemAvizhe.Services.Models.Digibile;

public class DigibileBaseResponse<T>
{
    public string success { get; set; }

    public string message { get; set; }

    public T result { get; set; }
}
