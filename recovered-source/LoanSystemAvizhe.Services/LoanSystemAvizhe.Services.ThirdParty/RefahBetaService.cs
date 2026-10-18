using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Services.Models.RefahBeta;
using Microsoft.Extensions.Logging;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class RefahBetaService
{
    private RefahBetaConfig _refahBetaConfig;

    private readonly ILogger<RefahBetaService> _logger;

    private string MainUrl { get; init; } = "https://api.rb24.ir/";

    private string BaseUrl => MainUrl + "beta/1.0/";

    public RefahBetaService(RefahBetaConfig refahBetaConfig, ILogger<RefahBetaService> logger)
    {
        _refahBetaConfig = refahBetaConfig;
        _logger = logger;
    }

    public async Task<RefahBetaResult<RefahBetaInquiryResultData>> InquiryRequest(RefahBetaInquiryRequest request)
    {
        RefahBetaTokenResultData refahBetaTokenResultData = await Token();
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refahBetaTokenResultData.access_token);
        client.DefaultRequestHeaders.Add("apikey", _refahBetaConfig.ApiKey);
        client.DefaultRequestHeaders.Add("terminalname", _refahBetaConfig.ClientId);
        using (HttpResponseMessage postResponse = await client.PostAsJsonAsync("credit/" + request.NationalCode + "/inquiry", request))
        {
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                string json = await postResponse.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<RefahBetaResult<RefahBetaInquiryResultData>>(json, options);
            }
            string result = postResponse.Content.ReadAsStringAsync().Result;
            _logger.LogError("InquiryRequest_errorContent: " + result);
        }
        throw new CustomException("خطا در ارتباط با سامانه بتا رفاه");
    }

    public async Task<RefahBetaResult<string>> OtpRequest(OtpRequest request)
    {
        RefahBetaTokenResultData refahBetaTokenResultData = await Token();
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refahBetaTokenResultData.access_token);
        client.DefaultRequestHeaders.Add("apikey", _refahBetaConfig.ApiKey);
        using (HttpResponseMessage postResponse = await client.PostAsJsonAsync("credit/" + request.NationalCode + "/request", request))
        {
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                string json = await postResponse.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<RefahBetaResult<string>>(json, options);
            }
        }
        throw new CustomException("خطا در ارتباط با سامانه بتا رفاه");
    }

    public async Task<RefahBetaResult<long>> ConsumeRequest(ConsumeRequest request)
    {
        RefahBetaTokenResultData refahBetaTokenResultData = await Token();
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refahBetaTokenResultData.access_token);
        client.DefaultRequestHeaders.Add("apikey", _refahBetaConfig.ApiKey);
        using (HttpResponseMessage postResponse = await client.PostAsJsonAsync("credit/" + request.NationalCode + "/consume", request))
        {
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                string json = await postResponse.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<RefahBetaResult<long>>(json, options);
            }
        }
        throw new CustomException("خطا در ارتباط با سامانه بتا رفاه");
    }

    private async Task<RefahBetaTokenResultData> Token()
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("client_id", _refahBetaConfig.ClientId);
        dictionary.Add("client_secret", _refahBetaConfig.ClientSecret);
        dictionary.Add("grant_type", "client_credentials");
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(MainUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        using (HttpResponseMessage postResponse = await client.PostAsync("connect/token", new FormUrlEncodedContent(dictionary)))
        {
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                string json = await postResponse.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                RefahBetaTokenResultData refahBetaTokenResultData = JsonSerializer.Deserialize<RefahBetaTokenResultData>(json, options);
                if (!string.IsNullOrWhiteSpace(refahBetaTokenResultData.access_token))
                {
                    return refahBetaTokenResultData;
                }
            }
            _ = postResponse.Content.ReadAsStringAsync().Result;
        }
        throw new CustomException("خطا در دریافت توکن");
    }
}
