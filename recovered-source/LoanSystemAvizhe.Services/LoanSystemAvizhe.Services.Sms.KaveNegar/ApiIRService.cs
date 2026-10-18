using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Services.Inquiry.Models.ApiIR;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms.KaveNegar;

public class ApiIRService : IInquiryService
{
    private ApiIRConfig _apiIRConfig;

    private readonly string BaseUrl = "https://s.api.ir/api/sw1/";

    public ApiIRService(ApiIRConfig apiIRConfig)
    {
        _apiIRConfig = apiIRConfig;
    }

    public ResponseInquiry<ShahkarResult> Shahkar(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<bool> sendApiIRResult = new SendApiIRResult<bool>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("ShahkarLite", sendInquiry as SendApiIR_Shahkar).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<bool>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<ShahkarResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new ShahkarResult
                        {
                            IsMatch = sendApiIRResult.Data
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<CreditResult> GetCredit()
    {
        throw new NotImplementedException();
    }

    public ResponseInquiry<ShahkarResult> Shahkar3(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_Shahkar3Result> sendApiIRResult = new SendApiIRResult<ApiIR_Shahkar3Result>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("Shahkar3", sendInquiry as SendApiIR_Shahkar).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_Shahkar3Result>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<ShahkarResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new ShahkarResult
                        {
                            Alive = sendApiIRResult.Data.Alive,
                            IsMatch = sendApiIRResult.Data.IsMatch
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<SanaResult> Sana(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<bool> sendApiIRResult = new SendApiIRResult<bool>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("Sana", sendInquiry as SendApiIR_Sana).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<bool>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<SanaResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new SanaResult
                        {
                            HasSana = sendApiIRResult.Data
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<UnpaidChequeResult> UnpaidCheque(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_UnpaidChequeResult> sendApiIRResult = new SendApiIRResult<ApiIR_UnpaidChequeResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("UnpaidCheque", sendInquiry as SendApiIR_UnpaidCheque).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_UnpaidChequeResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<UnpaidChequeResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new UnpaidChequeResult
                        {
                            Count = sendApiIRResult.Data.Count
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<PersonInfoResult> PersonInfo(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_PersonInfoResult> sendApiIRResult = new SendApiIRResult<ApiIR_PersonInfoResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("PersonInfo", sendInquiry as SendApiIR_PersonInfo).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_PersonInfoResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<PersonInfoResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new PersonInfoResult
                        {
                            NationalCode = sendApiIRResult.Data.NationalCode,
                            Alive = sendApiIRResult.Data.Alive,
                            FatherName = sendApiIRResult.Data.FatherName,
                            FirstName = sendApiIRResult.Data.FirstName,
                            Gender = sendApiIRResult.Data.Gender,
                            LastName = sendApiIRResult.Data.LastName
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<IbanInfoResult> IbanInfo(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_IbanInfoResult> sendApiIRResult = new SendApiIRResult<ApiIR_IbanInfoResult>();
        try
        {
            SendApiIR_IbanInfo sendApiIR_IbanInfo = sendInquiry as SendApiIR_IbanInfo;
            if (!sendApiIR_IbanInfo.Iban.ToLower().StartsWith("ir"))
            {
                sendApiIR_IbanInfo.Iban = "IR" + sendApiIR_IbanInfo.Iban;
            }
            HttpResponseMessage result = httpClient.PostAsJsonAsync("IbanInfo", sendInquiry as SendApiIR_IbanInfo).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_IbanInfoResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<IbanInfoResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new IbanInfoResult
                        {
                            Name = sendApiIRResult.Data.Name,
                            BankName = sendApiIRResult.Data.BankName,
                            Active = sendApiIRResult.Data.Active
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<BankCardInfoResult> BankCardInfo(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_BankCardInfoResult> sendApiIRResult = new SendApiIRResult<ApiIR_BankCardInfoResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("BankCardInfo", sendInquiry as SendApiIR_BankCardInfo).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_BankCardInfoResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<BankCardInfoResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new BankCardInfoResult
                        {
                            Name = sendApiIRResult.Data.Name,
                            AccountNumber = sendApiIRResult.Data.AccountNumber,
                            Iban = sendApiIRResult.Data.Iban
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<PostalAddressResult> PostalAddress(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_PostalAddressResult> sendApiIRResult = new SendApiIRResult<ApiIR_PostalAddressResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("PostalCodeInfo", sendInquiry as SendApiIR_PostalAddress).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_PostalAddressResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<PostalAddressResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new PostalAddressResult
                        {
                            BuildingName = sendApiIRResult.Data.BuildingName,
                            City = sendApiIRResult.Data.City,
                            District = sendApiIRResult.Data.District,
                            Floor = sendApiIRResult.Data.Floor,
                            Number = sendApiIRResult.Data.Number,
                            Province = sendApiIRResult.Data.Province,
                            SideFloor = sendApiIRResult.Data.SideFloor,
                            Street = sendApiIRResult.Data.Street,
                            Street2 = sendApiIRResult.Data.Street2,
                            Town = sendApiIRResult.Data.Town
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<List<ActivePlatesResult>> ActivePlates(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<List<ApiIR_ActivePlatesResult>> sendApiIRResult = new SendApiIRResult<List<ApiIR_ActivePlatesResult>>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("ActivePlates", sendInquiry as SendApiIR_ActivePlates).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<List<ApiIR_ActivePlatesResult>>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<List<ActivePlatesResult>>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = sendApiIRResult.Data.Select((ApiIR_ActivePlatesResult x) => new ActivePlatesResult
                        {
                            Code = x.Code,
                            IsFAK = x.IsFAK,
                            Description = x.Description,
                            Plate = x.Plate,
                            Serial = x.Serial
                        }).ToList()
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<CardMatchResult> CardMatch(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<bool> sendApiIRResult = new SendApiIRResult<bool>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("CardMatch", sendInquiry as SendApiIR_CardMatch).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<bool>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<CardMatchResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new CardMatchResult
                        {
                            IsMatch = sendApiIRResult.Data
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<IbanMatchResult> IbanMatch(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<bool> sendApiIRResult = new SendApiIRResult<bool>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("IbanMatch", sendInquiry as SendApiIR_IbanMatch).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<bool>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<IbanMatchResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new IbanMatchResult
                        {
                            IsMatch = sendApiIRResult.Data
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<ChequeColorResult> ChequeColor(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_ChequeColorResult> sendApiIRResult = new SendApiIRResult<ApiIR_ChequeColorResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("ChequeColor", sendInquiry as SendApiIR_ChequeColor).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_ChequeColorResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<ChequeColorResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new ChequeColorResult
                        {
                            ChequeColor = sendApiIRResult.Data.ChequeColor,
                            ChequeColorCode = sendApiIRResult.Data.ChequeColorCode
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<ActiveLoansResult> ActiveLoans(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_ActiveLoansResult> sendApiIRResult = new SendApiIRResult<ApiIR_ActiveLoansResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("ChequeColor", sendInquiry as SendApiIR_ActiveLoans).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_ActiveLoansResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<ActiveLoansResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new ActiveLoansResult
                        {
                            Count = sendApiIRResult.Data.Count,
                            DebtTotalAmount = sendApiIRResult.Data.DebtTotalAmount,
                            DeferredTotalAmount = sendApiIRResult.Data.DeferredTotalAmount,
                            Dishonored = sendApiIRResult.Data.Dishonored,
                            Name = sendApiIRResult.Data.Name,
                            PastExpiredTotalAmount = sendApiIRResult.Data.PastExpiredTotalAmount,
                            SuspiciousTotalAmount = sendApiIRResult.Data.SuspiciousTotalAmount,
                            TotalAmount = sendApiIRResult.Data.TotalAmount
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<ChequeInfoResult> ChequeInfo(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_ChequeInfoResult> sendApiIRResult = new SendApiIRResult<ApiIR_ChequeInfoResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("ChequeInfo", sendInquiry as SendApiIR_ChequeInfo).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_ChequeInfoResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<ChequeInfoResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new ChequeInfoResult
                        {
                            BranchCode = sendApiIRResult.Data.BranchCode,
                            ChequeType = sendApiIRResult.Data.ChequeType,
                            ExpirationDate = sendApiIRResult.Data.ExpirationDate,
                            Iban = sendApiIRResult.Data.Iban,
                            IssuedDate = sendApiIRResult.Data.IssuedDate,
                            Name = sendApiIRResult.Data.Name,
                            SerialNumber = sendApiIRResult.Data.SerialNumber,
                            SeriesNumber = sendApiIRResult.Data.SeriesNumber
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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

    public ResponseInquiry<MilitaryStatusResult> MilitaryStatus(BaseSendInquiry sendInquiry)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiIRConfig.ApiToken);
        SendApiIRResult<ApiIR_MilitaryStatusResult> sendApiIRResult = new SendApiIRResult<ApiIR_MilitaryStatusResult>();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("MilitaryStatus", sendInquiry as SendApiIR_MilitaryStatus).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendApiIRResult = JsonSerializer.Deserialize<SendApiIRResult<ApiIR_MilitaryStatusResult>>(result2, options);
                if (sendApiIRResult.Success)
                {
                    return new ResponseInquiry<MilitaryStatusResult>
                    {
                        Success = true,
                        Key = "",
                        Message = sendApiIRResult.Message,
                        Response = new MilitaryStatusResult
                        {
                            Qualified = sendApiIRResult.Data.Qualified
                        }
                    };
                }
                throw new CustomException("خطای صادر شده از سامانه استعلام: " + sendApiIRResult.Message);
            }
            _ = result.Content.ReadAsStringAsync().Result;
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
}
