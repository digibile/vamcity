using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystemAvizhe.Services.Sms.WebOne;

public class WebOneService : IPayamakService
{
    private WebOneConfig _webOneConfig;

    private readonly string BaseUrl = "https://api.payamakapi.ir/api/v1/";

    public WebOneService(WebOneConfig webOneConfig)
    {
        _webOneConfig = webOneConfig;
    }

    public ResponsePayamak Send(BaseSendPayamak sendPayamak)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _webOneConfig.ApiKey);
        SendWebOneResult sendWebOneResult = new SendWebOneResult();
        try
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(new
            {
                ToNumber = sendPayamak.To,
                Content = sendPayamak.Text,
                From = _webOneConfig.Sender
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = httpClient.PostAsync("SMS/Send", content).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendWebOneResult = JsonSerializer.Deserialize<SendWebOneResult>(result2, options);
                if (sendWebOneResult.ResultCode == 0)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = sendWebOneResult.RefId.ToString(),
                        Message = "",
                        Response = sendWebOneResult
                    };
                }
                throw new CustomException(sendWebOneResult.ResultMessage);
            }
            _ = result.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با وب وان");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال پیامک به وب سرویس وب وان");
        }
    }

    public ResponsePayamak SendByBaseNumber(BaseSendPayamak sendMelliPayamak, int bodyId)
    {
        throw new NotImplementedException();
    }

    public ResponsePayamak CheckDelivery(string messageId)
    {
        byte[] delivered = new byte[3] { 1, 3, 6 };
        _ = new byte[7] { 2, 4, 5, 7, 8, 9, 10 };
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _webOneConfig.ApiKey);
        DeliveryWebOneResult deliveryWebOneResult = new DeliveryWebOneResult();
        try
        {
            HttpResponseMessage result = httpClient.PostAsJsonAsync("SMS/Delivery?refId=" + messageId, new { }).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                deliveryWebOneResult = JsonSerializer.Deserialize<DeliveryWebOneResult>(result2, options);
                if (deliveryWebOneResult.Status)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = (deliveryWebOneResult.Deliveries.Any((DeliveryWebOneResultData x) => Enumerable.Contains(delivered, x.Delivery)) ? AppSmsStatus.Delivered.ToString() : AppSmsStatus.Pending.ToString()),
                        Message = deliveryWebOneResult.Message,
                        Response = deliveryWebOneResult
                    };
                }
                return new ResponsePayamak
                {
                    Success = false,
                    Key = "",
                    Message = deliveryWebOneResult.Message,
                    Response = null
                };
            }
            return new ResponsePayamak
            {
                Success = false,
                Key = "",
                Message = "",
                Response = null
            };
        }
        catch (Exception)
        {
            return new ResponsePayamak
            {
                Success = false,
                Key = "",
                Message = "",
                Response = null
            };
        }
    }

    public ResponsePayamak GetBasePrice(BaseSendPayamak sendMelliPayamak)
    {
        throw new NotImplementedException();
    }

    public ResponsePayamak GetCredit()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _webOneConfig.ApiKey);
        decimal num = 0m;
        try
        {
            HttpResponseMessage result = httpClient.GetAsync("SMS/GetCredit").Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                num = JsonSerializer.Deserialize<decimal>(result2, options);
                return new ResponsePayamak
                {
                    Success = true,
                    Key = (num / 10m).ToString(),
                    Message = "",
                    Response = num
                };
            }
            return new ResponsePayamak
            {
                Success = false,
                Key = "",
                Message = "",
                Response = null
            };
        }
        catch (Exception)
        {
            throw new CustomException("خطا در دریافت اطلاعات از سرور وب وان");
        }
    }

    public ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName)
    {
        throw new NotImplementedException();
    }

    public ResponsePayamak SendByPatternWebOne(SendWebOnePattern sendPayamak, string patternName)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-API-KEY", _webOneConfig.ApiKey);
        SendWebOneResult sendWebOneResult = new SendWebOneResult();
        try
        {
            StringContent content = new StringContent(JsonSerializer.Serialize(new
            {
                ToNumber = sendPayamak.To,
                Content = sendPayamak.Text,
                From = _webOneConfig.Sender
            }), Encoding.UTF8, "application/json");
            HttpResponseMessage result = httpClient.PostAsync("SMS/SmartOTP", content).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendWebOneResult = JsonSerializer.Deserialize<SendWebOneResult>(result2, options);
                if (sendWebOneResult.ResultCode == 0)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = sendWebOneResult.RefId.ToString(),
                        Message = "",
                        Response = sendWebOneResult
                    };
                }
                throw new CustomException(sendWebOneResult.ResultMessage);
            }
            _ = result.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با وب وان");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال پیامک به وب سرویس وب وان");
        }
    }

    public ResponsePayamak SendByPatternIPPanel(SendIPPanelPattern sendPayamak, string patternName)
    {
        throw new NotImplementedException();
    }
}
