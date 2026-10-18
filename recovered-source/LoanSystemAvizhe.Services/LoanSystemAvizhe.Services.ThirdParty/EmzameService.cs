using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Models.OnlineMoneyTransferPanel;
using LoanSystemAvizhe.Services.Models.Emzame;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class EmzameService
{
    private EmzameConfig _emzameConfig;

    private readonly string _baseUrl = "https://core.emzame.com/api/v1/backoffice/";

    public EmzameService(EmzameConfig emzameConfig)
    {
        _emzameConfig = emzameConfig;
    }

    public async Task<SendEmzameContractResult> SendContractAsync(SendEmzameContractRequest request, FileStream fileStream, bool useMock = false)
    {
        EmzameTokenResult emzameTokenResult = await Token();
        using HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(_baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", emzameTokenResult.accessToken);
        using MultipartFormDataContent content = new MultipartFormDataContent();
        content.Add(new StringContent(request.FileName ?? string.Empty), "fileName");
        content.Add(new StringContent(request.FileDescription ?? string.Empty), "fileDescription");
        content.Add(new StringContent(request.Title ?? string.Empty), "title");
        content.Add(new StringContent(request.Description ?? string.Empty), "description");
        content.Add(new StringContent(request.Provider ?? "gica"), "provider");
        content.Add(new StringContent(request.SignType ?? "e-sign"), "signType");
        content.Add(new StringContent(request.ContractType ?? "pdf"), "contractType");
        content.Add(new StringContent(request.IsElectronicSignatureRequired.ToString().ToLower()), "isElectronicSignatureRequired");
        content.Add(new StringContent(request.ShouldEveryPageHaveSignatureImage.ToString().ToLower()), "shouldEveryPageHaveSignatureImage");
        content.Add(new StringContent(request.IsSignatureDescriptionRequired.ToString().ToLower()), "isSignatureDescriptionRequired");
        content.Add(new StringContent(request.IsBlankPageRequire.ToString().ToLower()), "isBlankPageRequire");
        content.Add(new StringContent(request.ExpireTime.ToString()), "expireTime");
        foreach (SendEmzameContractCollaborator collaborator in request.CollaboratorList)
        {
            string content2 = JsonSerializer.Serialize(collaborator);
            content.Add(new StringContent(content2), "collaboratorList");
        }
        fileStream.Position = 0L;
        StreamContent streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(streamContent, "file", request.FileName ?? "contract.pdf");
        HttpResponseMessage response = await client.PostAsync("contracts/uploadFileAndSendContract", content);
        string text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new CustomException("Emzami Error | " + $"StatusCode: {(int)response.StatusCode} | " + "Response: " + text);
        }
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<SendEmzameContractResult>(text, options) ?? throw new CustomException("پاسخ دریافتی از امضامی معتبر نیست");
    }

    public async Task<SendEmzameContractResult> GetContractAsync(string contractId, bool useMock = false)
    {
        if (useMock)
        {
            SendEmzameContractResult sendEmzameContractResult = new SendEmzameContractResult();
            sendEmzameContractResult.collaborator = new EmzameContractCollaborator[1]
            {
                new EmzameContractCollaborator
                {
                    fullName = "علی حورشناس",
                    nationalCode = "231500505550",
                    status = "expired"
                }
            };
            sendEmzameContractResult.changedDate = 1716900000000L;
            sendEmzameContractResult.businessId = "22111212";
            sendEmzameContractResult.createdDate = DateTime.Now.Ticks;
            sendEmzameContractResult.expireTime = DateTime.Now.Ticks;
            sendEmzameContractResult.id = Guid.NewGuid().ToString();
            sendEmzameContractResult._id = Guid.NewGuid().ToString();
            sendEmzameContractResult.status = "expired";
            sendEmzameContractResult.title = "";
            sendEmzameContractResult.description = "";
            return sendEmzameContractResult;
        }
        EmzameTokenResult emzameTokenResult = await Token();
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_baseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", emzameTokenResult.accessToken);
        new SendEmzameContractResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("contracts/" + contractId);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<SendEmzameContractResult>(result, options);
            }
            _ = httpResponseMessage.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با سامانه امضامی");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال اطلاعات به سامانه امضامی");
        }
    }

    public async Task<SendGetFileResult> GetFile(string contractId)
    {
        EmzameTokenResult emzameTokenResult = await Token();
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_baseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        new SendGetFileResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("files/" + emzameTokenResult.userInfo.validFileLink + "/" + contractId);
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string result = httpResponseMessage.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<SendGetFileResult>(result, options);
            }
            _ = httpResponseMessage.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با سامانه امضامی");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال اطلاعات به سامانه امضامی");
        }
    }

    private async Task<EmzameTokenResult> Token()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(_baseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("login", new SendEmzameTokenRequest
        {
            NationalCode = _emzameConfig.NationalCode,
            Password = _emzameConfig.Password
        });
        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            EmzameTokenResult emzameTokenResult = JsonSerializer.Deserialize<EmzameTokenResult>(json, options);
            if (!string.IsNullOrEmpty(emzameTokenResult.accessToken))
            {
                return emzameTokenResult;
            }
            throw new CustomException("خطا در دریافت توکن: " + emzameTokenResult.message);
        }
        throw new CustomException("خطا در دریافت توکن");
    }
}
