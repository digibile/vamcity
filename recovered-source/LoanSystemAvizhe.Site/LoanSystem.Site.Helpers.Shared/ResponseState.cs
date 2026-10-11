namespace LoanSystem.Site.Helpers.Shared;

public class ResponseState<T>
{
	public bool Success { get; set; }

	public string Message { get; set; }

	public ResponseStateMessageTypes MessageType { get; set; }

	public T Data { get; set; }
}
