namespace LoanSystem.Services.Models.Landa;

public class LandaResponse<T>
{
    public int code { get; set; }

    public string message { get; set; }

    public T data { get; set; }
}
