using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystemAvizhe.Services.Sms.IPPanel;

public class IPPanelService : IPayamakService
{
    private IPPanelConfig _ippanelConfig;

    private readonly string BaseUrl = "https://api2.ippanel.com/api/v1/sms/";

    public IPPanelService(IPPanelConfig ipPanelConfig)
    {
        _ippanelConfig = ipPanelConfig;
    }

    public ResponsePayamak Send(BaseSendPayamak sendPayamak)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("apikey", _ippanelConfig.ApiKey);
        SendIPPanelResult sendIPPanelResult = new SendIPPanelResult();
        try
        {
            SendIPPanel sendIPPanel = new SendIPPanel();
            sendIPPanel.Recipient = new string[1] { "+98" + sendPayamak.To.TrimStart('0') };
            sendIPPanel.Sender = _ippanelConfig.Sender;
            sendIPPanel.Message = sendPayamak.Text;
            sendIPPanel.Time = "";
            SendIPPanel value = sendIPPanel;
            HttpResponseMessage result = httpClient.PostAsJsonAsync("send/webservice/single", value).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendIPPanelResult = JsonSerializer.Deserialize<SendIPPanelResult>(result2, options);
                if (sendIPPanelResult.Code == 200)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = sendIPPanelResult.Data.Message_id.ToString(),
                        Message = sendIPPanelResult.Status,
                        Response = sendIPPanelResult
                    };
                }
                throw new CustomException(sendIPPanelResult.ErrorMessage);
            }
            _ = result.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با آی پی پنل");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال پیامک به وب سرویس آی پی پنل");
        }
    }

    public ResponsePayamak SendByBaseNumber(BaseSendPayamak sendMelliPayamak, int bodyId)
    {
        throw new NotImplementedException();
    }

    public ResponsePayamak CheckDelivery(string messageId)
    {
        byte[] delivered = new byte[1] { 2 };
        _ = new byte[2] { 3, 4 };
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("apikey", _ippanelConfig.ApiKey);
        DeliveryIPPanelResult deliveryIPPanelResult = new DeliveryIPPanelResult();
        try
        {
            HttpResponseMessage result = httpClient.GetAsync("message/show-recipient/message-id/" + messageId + "?page=1&per_page=1").Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                deliveryIPPanelResult = JsonSerializer.Deserialize<DeliveryIPPanelResult>(result2, options);
                if (deliveryIPPanelResult.Code == 200)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = (deliveryIPPanelResult.Data.Deliveries.Any((DeliveryIPPanelResultDataDelivery x) => Enumerable.Contains(delivered, x.Status)) ? AppSmsStatus.Delivered.ToString() : AppSmsStatus.Pending.ToString()),
                        Message = deliveryIPPanelResult.Status,
                        Response = deliveryIPPanelResult
                    };
                }
                return new ResponsePayamak
                {
                    Success = false,
                    Key = "",
                    Message = deliveryIPPanelResult.ErrorMessage,
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
        httpClient.DefaultRequestHeaders.Add("apikey", _ippanelConfig.ApiKey);
        SendIPPanelResult sendIPPanelResult = new SendIPPanelResult();
        try
        {
            HttpResponseMessage result = httpClient.GetAsync("accounting/credit/show").Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendIPPanelResult = JsonSerializer.Deserialize<SendIPPanelResult>(result2, options);
                if (sendIPPanelResult.Code == 200)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = (sendIPPanelResult.Data.Credit / 10).ToString(),
                        Message = sendIPPanelResult.Status,
                        Response = sendIPPanelResult
                    };
                }
                return new ResponsePayamak
                {
                    Success = false,
                    Key = "",
                    Message = sendIPPanelResult.ErrorMessage,
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
            throw new CustomException("خطا در دریافت اطلاعات از سرور آی پی پنل");
        }
    }

    public ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName)
    {
        throw new NotImplementedException();
    }

    public ResponsePayamak SendByPatternIPPanel(SendIPPanelPattern sendPayamak, string patternName)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("apikey", _ippanelConfig.ApiKey);
        SendIPPanelResult sendIPPanelResult = new SendIPPanelResult();
        try
        {
            SendIPPanelPattern value = new SendIPPanelPattern
            {
                Code = patternName,
                Recipient = "+98" + sendPayamak.To.TrimStart('0'),
                Sender = _ippanelConfig.Sender,
                Variable = sendPayamak.Variable
            };
            HttpResponseMessage result = httpClient.PostAsJsonAsync("pattern/normal/send", value).Result;
            if (result.StatusCode == HttpStatusCode.OK)
            {
                string result2 = result.Content.ReadAsStringAsync().Result;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                sendIPPanelResult = JsonSerializer.Deserialize<SendIPPanelResult>(result2, options);
                if (sendIPPanelResult.Code == 200)
                {
                    return new ResponsePayamak
                    {
                        Success = true,
                        Key = sendIPPanelResult.Data.Message_id.ToString(),
                        Message = sendIPPanelResult.Status,
                        Response = sendIPPanelResult
                    };
                }
                throw new CustomException(sendIPPanelResult.ErrorMessage);
            }
            _ = result.Content.ReadAsStringAsync().Result;
            throw new CustomException("خطا در ارتباط با آی پی پنل");
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CustomException("خطا در ارسال پیامک به وب سرویس آی پی پنل");
        }
    }

    public ResponsePayamak SendByPatternWebOne(SendWebOnePattern sendPayamak, string patternName)
    {
        throw new NotImplementedException();
    }
}
