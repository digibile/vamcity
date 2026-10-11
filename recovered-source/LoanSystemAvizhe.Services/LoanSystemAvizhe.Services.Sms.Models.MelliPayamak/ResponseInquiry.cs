namespace LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

public class ResponseInquiry<T>
{
	public bool Success { get; set; }

	public string Message { get; set; }

	public string Key { get; set; }

	public T Response { get; set; }
}
