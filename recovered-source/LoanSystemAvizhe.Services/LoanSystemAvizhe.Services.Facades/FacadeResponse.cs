namespace LoanSystemAvizhe.Services.Facades;

public class FacadeResponse<T>
{
	public bool Success { get; set; }

	public string Message { get; set; }

	public T Data { get; set; }
}
