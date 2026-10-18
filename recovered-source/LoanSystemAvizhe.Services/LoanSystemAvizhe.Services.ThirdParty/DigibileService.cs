using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystemAvizhe.Services.Models.Digibile;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class DigibileService
{
    private readonly IThirdPartySettingsService _thirdPartySettingsService;

    private readonly string BaseUrl = "https://digibile.com/api/v1/";

    public DigibileService(IThirdPartySettingsService thirdPartySettingsService)
    {
        _thirdPartySettingsService = thirdPartySettingsService;
    }

    public async Task<DigibileBaseResponse<RegisterUserResponse>> RegisterUser(RegisterUser request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
        });
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        DigibileBaseResponse<RegisterUserResponse> result = new DigibileBaseResponse<RegisterUserResponse>();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("UserAPI.php?action=register", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<DigibileBaseResponse<RegisterUserResponse>>(json, options);
                return result;
            }
            result.message = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return result;
        }
        catch
        {
            return result;
        }
    }

    public async Task<DigibileBaseResponse<ShowUserResponse>> ShowUser(ShowUser request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
        });
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        DigibileBaseResponse<ShowUserResponse> result = new DigibileBaseResponse<ShowUserResponse>();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("UserAPI.php?action=show_user", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<DigibileBaseResponse<ShowUserResponse>>(json, options);
                return result;
            }
            return result;
        }
        catch
        {
            return result;
        }
    }

    public async Task<DigibileBaseResponse<ShowUsersResponse>> ShowUsers(ShowUsers request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
        });
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        DigibileBaseResponse<ShowUsersResponse> result = new DigibileBaseResponse<ShowUsersResponse>();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("UserAPI.php?action=show_users", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<DigibileBaseResponse<ShowUsersResponse>>(json, options);
                return result;
            }
            result.message = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return result;
        }
        catch
        {
            return result;
        }
    }

    public async Task<DigibileBaseResponse<UpdateUserResponse>> UpdateUser(UpdateUser request)
    {
        string parameter = await Token();
        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
        });
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
        DigibileBaseResponse<UpdateUserResponse> result = new DigibileBaseResponse<UpdateUserResponse>();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("UserAPI.php?action=update_user", request);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<DigibileBaseResponse<UpdateUserResponse>>(json, options);
                return result;
            }
            result.message = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return result;
        }
        catch
        {
            return result;
        }
    }

    private async Task<string> Token()
    {
        ThirdPartySettingsModel thirdPartySettingsModel = new ThirdPartySettingsModel((await _thirdPartySettingsService.GetThirdPartySettings()) ?? throw new CustomException("خطا در دریافت توکن: توکن در تنظیمات وام سیتی ثبت نشده است."));
        HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
        });
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("UserAPI.php?action=get_token", new DigibileTokenRequest
            {
                secret_key = thirdPartySettingsModel.DigibileSettings.SecretKey
            });
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                DigibileTokenResult digibileTokenResult = JsonSerializer.Deserialize<DigibileTokenResult>(json, options);
                if (digibileTokenResult.success)
                {
                    return digibileTokenResult.token;
                }
                throw new CustomException("خطا در دریافت توکن: " + digibileTokenResult.message);
            }
        }
        catch (Exception)
        {
        }
        throw new CustomException("خطا در دریافت توکن: نامشخص");
    }
}
