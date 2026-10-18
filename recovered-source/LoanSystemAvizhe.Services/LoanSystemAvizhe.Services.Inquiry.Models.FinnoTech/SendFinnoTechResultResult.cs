using System.Text.Json;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class SendFinnoTechResultResult<T>
{
    public T result { get; set; }

    public string status { get; set; }

    public string trackId { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
