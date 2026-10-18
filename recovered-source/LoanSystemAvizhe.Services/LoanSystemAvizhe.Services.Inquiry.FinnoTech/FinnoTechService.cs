using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Services.Inquiry.Models.FinnoTech;
using LoanSystemAvizhe.Services.Sms;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Inquiry.FinnoTech;

public class FinnoTechService : IInquiryService
{
    private readonly FinnoTechConfig _finnoTechConfig;

    private readonly string BaseUrl = "https://apibeta.finnotech.ir";

    public FinnoTechService(FinnoTechConfig finnoTechConfig)
    {
        _finnoTechConfig = finnoTechConfig;
    }

    public ResponseInquiry<ActiveLoansResult> ActiveLoans(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<List<ActivePlatesResult>> ActivePlates(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<BankCardInfoResult> BankCardInfo(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<CardMatchResult> CardMatch(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<ChequeColorResult> ChequeColor(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<ChequeInfoResult> ChequeInfo(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<CreditResult> GetCredit()
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<IbanInfoResult> IbanInfo(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<IbanMatchResult> IbanMatch(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<MilitaryStatusResult> MilitaryStatus(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<PersonInfoResult> PersonInfo(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<PostalAddressResult> PostalAddress(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<SanaResult> Sana(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<ShahkarResult> Shahkar(BaseSendInquiry sendInquiry)
    {
        string result = Token().Result;
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("clientId", _finnoTechConfig.ClientId);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result);
        SendFinnoTechResultResult<FinnoTech_ShahkarResult> sendFinnoTechResultResult = new SendFinnoTechResultResult<FinnoTech_ShahkarResult>();
        try
        {
            HttpResponseMessage result2 = httpClient.GetAsync("/facility/v2/clients/" + _finnoTechConfig.ClientId + "/shahkar/verify?trackId=" + (sendInquiry as SendFinnoTech_Shahkar).TrackId + "&mobile=" + (sendInquiry as SendFinnoTech_Shahkar).Mobile + "&nationalCode=" + (sendInquiry as SendFinnoTech_Shahkar).NationalCode).Result;
            if (result2.StatusCode == HttpStatusCode.OK)
            {
                string result3 = result2.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendFinnoTechResultResult = JsonSerializer.Deserialize<SendFinnoTechResultResult<FinnoTech_ShahkarResult>>(result3, options);
                if (sendFinnoTechResultResult.result != null)
                {
                    return new ResponseInquiry<ShahkarResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendFinnoTechResultResult.status,
                        Response = new ShahkarResult
                        {
                            IsMatch = sendFinnoTechResultResult.result.isValid
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendFinnoTechResultResult.status);
            }
            _ = result2.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با سامانه استعلام");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال اطلاعات به سامانه استعلام");
        }
    }

    public ResponseInquiry<ShahkarResult> Shahkar3(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<UnpaidChequeResult> UnpaidCheque(BaseSendInquiry sendInquiry)
    {
        throw new NotImplementedException();
    }

    private async Task<string> Token()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(_finnoTechConfig.ClientId + ":" + _finnoTechConfig.ClientSecret));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", parameter);
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/dev/v2/oauth2/token", new FinnoTechTokenRequest
        {
            grant_type = "client_credentials",
            nid = _finnoTechConfig.NId,
            scopes = "facility:shahkar:get"
        });
        if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
        {
            string json = await httpResponseMessage.Content.ReadAsStringAsync();
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            FinnoTechTokenResponse finnoTechTokenResponse = JsonSerializer.Deserialize<FinnoTechTokenResponse>(json, options);
            if (finnoTechTokenResponse.status.ToLower() == "done")
            {
                return finnoTechTokenResponse.result.value;
            }
            throw new CustomException("خطا در دریافت توکن: " + finnoTechTokenResponse.error);
        }
        throw new CustomException("خطا در دریافت توکن: نامشخص");
    }
}
