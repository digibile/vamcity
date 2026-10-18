using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Services.Models.Sepehr;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class SepehrService
{
    private readonly string BaseUrl = "https://sepehr.shaparak.ir/Rest/V1/PeymentApi/";

    private readonly ILogger<SepehrService> _logger;

    public SepehrService(ILogger<SepehrService> logger)
    {
        _logger = logger;
    }

    public async Task<(string, SepehrRequestResult)> Request(SepehrRequest request)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SepehrRequestResult result = new SepehrRequestResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("GetToken", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("SepehrRequest_postResponse: " + httpResponseMessage.Content.ReadAsStringAsync().Result);
                result = await httpResponseMessage.Content.ReadFromJsonAsync<SepehrRequestResult>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return ($"https://sepehr.shaparak.ir/Payment/Pay?token={result.Accesstoken}&terminalId=${request.TerminalId}", result);
            }
            string result2 = httpResponseMessage.Content.ReadAsStringAsync().Result;
            _logger.LogError("SepehrRequest_errorContent: " + result2);
            return ("", result);
        }
        catch (Exception ex)
        {
            _logger.LogError("SepehrRequest_exception: " + ex.Message);
            return ("", result);
        }
    }

    public async Task<SepehrVerifyResult> Verify(SepehrVerify verification)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        SepehrVerifyResult result = new SepehrVerifyResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("Advice", verification);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                result = (await httpResponseMessage.Content.ReadFromJsonAsync<SepehrVerifyResult>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })) ?? result;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }
}
