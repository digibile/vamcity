using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Services.Models.ZarinPal;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class ZarinPalService
{
    private readonly string BaseUrl = "https://{0}.zarinpal.com/pg/v4/payment/";

    public async Task<(string, ZarinPalRequestResult)> Request(ZarinPalRequestWage request, ZarinPalRequestMode mode)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(string.Format(BaseUrl, mode.ToString()));
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        ZarinPalRequestResult result = new ZarinPalRequestResult();
        try
        {
            HttpResponseMessage httpResponseMessage;
            if (request.wages?.Any() ?? false)
            {
                httpResponseMessage = await httpClient.PostAsJsonAsync("request.json", request);
            }
            else if (string.IsNullOrEmpty(request.metadata?.mobile))
            {
                ZarinPalRequestNoMetaData value = JsonSerializer.Deserialize<ZarinPalRequestNoMetaData>(JsonSerializer.Serialize(request));
                httpResponseMessage = await httpClient.PostAsJsonAsync("request.json", value);
            }
            else
            {
                ZarinPalRequest value2 = JsonSerializer.Deserialize<ZarinPalRequest>(JsonSerializer.Serialize(request));
                httpResponseMessage = await httpClient.PostAsJsonAsync("request.json", value2);
            }
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<ZarinPalRequestResult>(json, options);
                return ("https://" + ((mode == ZarinPalRequestMode.sandbox) ? "sandbox" : "www") + ".zarinpal.com/pg/StartPay/" + result.data.authority, result);
            }
            return ("", result);
        }
        catch (Exception)
        {
            return ("", result);
        }
    }

    public async Task<ZarinPalVerifyResult> Verify(ZarinPalVerify verification, ZarinPalRequestMode mode)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(string.Format(BaseUrl, mode.ToString()));
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        ZarinPalVerifyResult result = new ZarinPalVerifyResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("verify.json", verification);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<ZarinPalVerifyResult>(json, options);
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }
}
