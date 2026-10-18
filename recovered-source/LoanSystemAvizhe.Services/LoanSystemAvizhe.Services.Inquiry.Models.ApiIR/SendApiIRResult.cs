namespace LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;

public class SendApiIRResult<T>
{
    public bool Success { get; set; }

    public string Error { get; set; }

    public string Message { get; set; }

    public int Code { get; set; }

    public T Data { get; set; }
}
