using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Base;
using LoanSystem.Services.Models;
using LoanSystem.Services.Models.Landa;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class LandaService
{
	private readonly IThirdPartySettingsService _thirdPartySettingsService;

	private readonly string BaseUrl = "https://my.landa-sme.ir/public/api/";

	public LandaService(IThirdPartySettingsService thirdPartySettingsService)
	{
		_thirdPartySettingsService = thirdPartySettingsService;
	}

	public async Task<LandaResponse<CreateCustomerResponse>> CreateCustomer(CreateCustomer request)
	{
		string value = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("token", value);
		LandaResponse<CreateCustomerResponse> result = new LandaResponse<CreateCustomerResponse>();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("Customer/createCustomer", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<LandaResponse<CreateCustomerResponse>>(json, options);
				return result;
			}
			result.message = "خطا در ارسال اطلاعات به سرور لاندا: " + httpResponseMessage.Content.ReadAsStringAsync().Result;
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	public async Task<GetCustomerResponse> GetCustomerByCode(string code)
	{
		string value = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("token", value);
		new LandaPagedResponse<GetCustomerResponse>();
		try
		{
			GetCustomerRequest value2 = new GetCustomerRequest
			{
				currentPage = 1,
				pageSize = 100000
			};
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "Customer/getCustomers");
			httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(value2, options), Encoding.UTF8, "application/json");
			httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				return JsonSerializer.Deserialize<LandaPagedResponse<GetCustomerResponse>>(await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), options)?.items?.FirstOrDefault((GetCustomerResponse x) => x.code == code);
			}
			return null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public async Task<List<GetCustomerResponse>> GetAllCustomers()
	{
		string value = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("token", value);
		new LandaPagedResponse<GetCustomerResponse>();
		try
		{
			GetCustomerRequest value2 = new GetCustomerRequest
			{
				currentPage = 1,
				pageSize = 100000
			};
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "Customer/getCustomers");
			httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(value2, options), Encoding.UTF8, "application/json");
			httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				return JsonSerializer.Deserialize<LandaPagedResponse<GetCustomerResponse>>(await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false), options)?.items;
			}
			return new List<GetCustomerResponse>();
		}
		catch (Exception)
		{
			return new List<GetCustomerResponse>();
		}
	}

	public async Task<LandaResponse<CreateInvoiceResponse>> CreateInvoice(CreateInvoice request)
	{
		string value = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("token", value);
		LandaResponse<CreateInvoiceResponse> result = new LandaResponse<CreateInvoiceResponse>();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("invoice/createInvoice", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<LandaResponse<CreateInvoiceResponse>>(json, options);
				return result;
			}
			result.message = "خطا در ارسال اطلاعات به سرور لاندا: " + httpResponseMessage.Content.ReadAsStringAsync().Result;
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	public async Task<LandaResponse<CreateInvoicePaymentResponse>> CreateInvoicePayment(CreateInvoicePayment request)
	{
		string value = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("token", value);
		LandaResponse<CreateInvoicePaymentResponse> result = new LandaResponse<CreateInvoicePaymentResponse>();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("invoice/createPayment", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<LandaResponse<CreateInvoicePaymentResponse>>(json, options);
				return result;
			}
			result.message = "خطا در ارسال اطلاعات به سرور لاندا: " + httpResponseMessage.Content.ReadAsStringAsync().Result;
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	private async Task<string> Token()
	{
		ThirdPartySettingsModel thirdPartySettingsModel = new ThirdPartySettingsModel((await _thirdPartySettingsService.GetThirdPartySettings()) ?? throw new CustomException("خطا در دریافت توکن: توکن در تنظیمات لاندا ثبت نشده است."));
		if (string.IsNullOrEmpty(thirdPartySettingsModel?.LandaSettings?.Token))
		{
			throw new CustomException("خطا در دریافت توکن: توکن در تنظیمات لاندا ثبت نشده است.");
		}
		return thirdPartySettingsModel.LandaSettings.Token;
	}
}
