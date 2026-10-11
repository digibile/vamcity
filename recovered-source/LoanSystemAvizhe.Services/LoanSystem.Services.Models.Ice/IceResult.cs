namespace LoanSystem.Services.Models.Ice;

public class IceResult<T> where T : class
{
	public T Data { get; set; }

	public int State { get; set; }

	public string[] Errors { get; set; }

	public int ErrorCode { get; set; }
}
