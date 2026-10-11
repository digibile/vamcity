using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Data.Enums;
using LoanSystemAvizhe.Services.Sms.Models.Bale;
using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystemAvizhe.Services.Sms.Bale;

public class BaleApiService : IPayamakService
{
	private BaleConfig _baleConfig;

	private readonly string BaseUrl = "https://safir.bale.ai/api/v3/";

	public BaleApiService(BaleConfig baleConfig)
	{
		_baleConfig = baleConfig;
	}

	public ResponsePayamak Send(BaseSendPayamak sendPayamak)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("api-access-key", _baleConfig.ApiKey);
		SendBaleMessageResult sendBaleMessageResult = new SendBaleMessageResult();
		try
		{
			sendPayamak.Text = sendPayamak.Text.Replace("{", "").Replace("}", "");
			BaleMessageData value = new BaleMessageData
			{
				bot_id = _baleConfig.BotId,
				phone_number = NormalizePhoneNumber(sendPayamak.To),
				request_id = "",
				message_data = (((sendPayamak as SendBale).Otp != 0) ? new MessageDataModel((sendPayamak as SendBale).Otp.ToString()) : new MessageDataModel(new MessageModel(sendPayamak.Text)))
			};
			HttpResponseMessage result = httpClient.PostAsJsonAsync("send_message", value).Result;
			if (result.StatusCode == HttpStatusCode.OK)
			{
				string result2 = result.Content.ReadAsStringAsync().Result;
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				sendBaleMessageResult = JsonSerializer.Deserialize<SendBaleMessageResult>(result2, options);
				if (sendBaleMessageResult.success)
				{
					return new ResponsePayamak
					{
						Success = true,
						Key = sendBaleMessageResult.message_id,
						Message = "",
						Response = sendBaleMessageResult
					};
				}
				throw new CustomException(string.Join(',', sendBaleMessageResult.error_data?.Select((SendBaleErrorInfo x) => x.description).ToList()));
			}
			if (result.StatusCode == HttpStatusCode.BadRequest)
			{
				string result3 = result.Content.ReadAsStringAsync().Result;
				JsonSerializerOptions options2 = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				sendBaleMessageResult = JsonSerializer.Deserialize<SendBaleMessageResult>(result3, options2);
				throw new CustomException(string.Join(',', sendBaleMessageResult.error_data?.Select((SendBaleErrorInfo x) => x.description).ToList()));
			}
			throw new CustomException(result.Content.ReadAsStringAsync().Result);
		}
		catch (CustomException)
		{
			throw;
		}
		catch (Exception)
		{
			throw new CustomException("خطا در ارسال پیام به سرور پیام رسان بله");
		}
	}

	public ResponsePayamak SendByBaseNumber(BaseSendPayamak sendMelliPayamak, int bodyId)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak CheckDelivery(string messageId)
	{
		return new ResponsePayamak
		{
			Success = true,
			Key = AppSmsStatus.Delivered.ToString(),
			Message = "",
			Response = new { }
		};
	}

	public ResponsePayamak GetBasePrice(BaseSendPayamak sendMelliPayamak)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak GetCredit()
	{
		return new ResponsePayamak
		{
			Success = true,
			Key = "",
			Message = "",
			Response = 0
		};
	}

	public ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak SendByPatternIPPanel(SendIPPanelPattern sendPayamak, string patternName)
	{
		throw new NotImplementedException();
	}

	public ResponsePayamak SendByPatternWebOne(SendWebOnePattern sendPayamak, string patternName)
	{
		throw new NotImplementedException();
	}

	private string NormalizePhoneNumber(string phoneNumber)
	{
		if (!phoneNumber.StartsWith("+98") && !phoneNumber.StartsWith("98"))
		{
			phoneNumber = "98" + (phoneNumber.StartsWith("0") ? phoneNumber.Substring(1, phoneNumber.Length - 1) : phoneNumber);
		}
		if (phoneNumber.StartsWith("+980"))
		{
			phoneNumber = phoneNumber.Replace("+980", "+98");
		}
		if (phoneNumber.StartsWith("980"))
		{
			phoneNumber = phoneNumber.Replace("980", "98");
		}
		return phoneNumber;
	}
}
