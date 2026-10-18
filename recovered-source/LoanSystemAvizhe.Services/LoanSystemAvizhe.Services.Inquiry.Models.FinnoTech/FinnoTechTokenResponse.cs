using System.Text.Json;

namespace LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;

public class FinnoTechTokenResponse
{
    public FinnoTechTokenResult result { get; set; }

    public string error { get; set; }

    public string status { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
