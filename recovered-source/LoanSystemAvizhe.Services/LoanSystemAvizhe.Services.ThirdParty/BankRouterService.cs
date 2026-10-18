using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LoanSystem.Services.Models.Sep;
using LoanSystem.Services.Models.Sepehr;
using LoanSystemAvizhe.Services.Models.Sadad;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class BankRouterService
{
    private readonly string BaseUrl = "";

    public BankRouterService(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public async Task<(string, SepRequestResult)> SepRequest(SepRequest request)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SepRequestResult result = new SepRequestResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/start/SepToken", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = (result.ErrorDesc = await httpResponseMessage.Content.ReadAsStringAsync());
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<SepRequestResult>(json, options);
                return (BaseUrl.TrimEnd('/') + "/start/sep?token=" + result.Token, result);
            }
            return ("", result);
        }
        catch (Exception)
        {
            return ("", result);
        }
    }

    public async Task<(string, SepehrRequestResult)> SepehrRequest(SepehrRequest request)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SepehrRequestResult result = new SepehrRequestResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/start/SepehrToken", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<SepehrRequestResult>(json, options);
                return (BaseUrl.TrimEnd('/') + "/start/sepehr?token=" + result.Accesstoken, result);
            }
            return ("", result);
        }
        catch (Exception)
        {
            return ("", result);
        }
    }

    public async Task<(string, SadadRequestResult)> SadadRequest(SadadPaymentRequest request)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SadadRequestResult result = new SadadRequestResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/start/SadadToken", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<SadadRequestResult>(json, options);
                return ("https://sadad.shaparak.ir/Purchase/Index?token=" + result.Token, result);
            }
            return ("", result);
        }
        catch (Exception)
        {
            return ("", result);
        }
    }

    public async Task<SepVerifyResult> Verify(SepVerify verification)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SepVerifyResult result = new SepVerifyResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/verify/sep", verification);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<SepVerifyResult>(json, options);
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    public async Task<SadadVerifyResult> SadadVerify(SadadVerifyRequest verification)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SadadVerifyResult result = new SadadVerifyResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/verify/sadad", verification);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                return (await httpResponseMessage.Content.ReadFromJsonAsync<SadadVerifyResult>((JsonSerializerOptions?)null, default(CancellationToken))) ?? result;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }
}
