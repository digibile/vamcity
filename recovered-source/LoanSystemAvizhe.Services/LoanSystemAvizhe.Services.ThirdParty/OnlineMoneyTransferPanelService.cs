using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Models.EFarda;
using LoanSystem.Services.Models.OnlineMoneyTransferPanel;
using Microsoft.Extensions.Options;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class OnlineMoneyTransferPanelService
{
    private readonly OnlineMoneyTransferPanelConfig _OnlineMoneyTransferPanelConfig;

    public OnlineMoneyTransferPanelService(IOptions<OnlineMoneyTransferPanelConfig> OnlineMoneyTransferPanelConfig)
    {
        _OnlineMoneyTransferPanelConfig = OnlineMoneyTransferPanelConfig.Value;
    }

    public async Task<OnlineMoneyTransferPanelInitAndSendOTPResult> InitAndSendOTP(OnlineMoneyTransferPanelInitAndSendOTP request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_OnlineMoneyTransferPanelConfig.BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        OnlineMoneyTransferPanelInitAndSendOTPResult result = new OnlineMoneyTransferPanelInitAndSendOTPResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/Transfer/InitAndSendOTP", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<OnlineMoneyTransferPanelInitAndSendOTPResult>(json, options);
                return result;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    public async Task<OnlineMoneyTransferPanelInquiryResult> IbanInquiry(OnlineMoneyTransferPanelInquiry request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_OnlineMoneyTransferPanelConfig.BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        OnlineMoneyTransferPanelInquiryResult result = new OnlineMoneyTransferPanelInquiryResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/Transfer/IbanInquiry", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<OnlineMoneyTransferPanelInquiryResult>(json, options);
                return result;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    public async Task<OnlineMoneyTransferPanelCompleteTransferResult> CompleteTransfer(OnlineMoneyTransferPanelCompleteTransfer request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_OnlineMoneyTransferPanelConfig.BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        OnlineMoneyTransferPanelCompleteTransferResult result = new OnlineMoneyTransferPanelCompleteTransferResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/Transfer/CompleteTransfer", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<OnlineMoneyTransferPanelCompleteTransferResult>(json, options);
                return result;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    private async Task<string> Token()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_OnlineMoneyTransferPanelConfig.BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/Account/Token", new OnlineMoneyTransferPanelTokenRequest
        {
            UserName = _OnlineMoneyTransferPanelConfig.Username,
            Password = _OnlineMoneyTransferPanelConfig.Password
        });
        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            OnlineMoneyTransferPanelTokenTokenResultData onlineMoneyTransferPanelTokenTokenResultData = JsonSerializer.Deserialize<OnlineMoneyTransferPanelTokenTokenResultData>(json, options);
            if (onlineMoneyTransferPanelTokenTokenResultData.success)
            {
                return onlineMoneyTransferPanelTokenTokenResultData.result.token;
            }
            throw new CustomException("خطا در دریافت توکن: " + onlineMoneyTransferPanelTokenTokenResultData.message);
        }
        throw new CustomException("خطا در دریافت توکن: نامشخص");
    }
}
