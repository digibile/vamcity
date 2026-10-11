using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Models.IranValidity;
using LoanSystemAvizhe.Services.Models.IranValidity;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class IranValidityService
{
	private readonly string BaseUrl = "https://app.ics24.ir/";

	private IranValidityConfig _iranValidityConfig;

	private bool _developMode;

	public IranValidityService(IranValidityConfig iranValidityConfig, bool developMode)
	{
		_iranValidityConfig = iranValidityConfig;
		_developMode = developMode;
	}

	public async Task<IranValidityAddFromFirmResultData> FromFirmRequest(IranValidityFromFirmRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("RealPersonNationalCode", request.RealPersonNationalCode);
		dictionary.Add("MobileNumber", request.MobileNumber);
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.PostAsync("b2b/api/request", new FormUrlEncodedContent(dictionary)))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<string> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<string>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return new IranValidityAddFromFirmResultData
					{
						ExpireDate = DateTime.Now.AddHours(24.0),
						HashCode = iranValidityResult.Data
					};
				}
				throw new CustomException(string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList()));
			}
		}
		throw new CustomException("خطا در ارسال اطلاعات اعتبارسنجی به سرور اعتبارسنجی ایران");
	}

	public async Task<IranValidityAddFromFirmResultData> RenewOTP(IranValidityRenewOTPRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Code", request.HashCode);
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.PostAsync("b2b/api/request/" + request.HashCode + "/RenewToken", new FormUrlEncodedContent(dictionary)))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<string> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<string>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return new IranValidityAddFromFirmResultData
					{
						ExpireDate = DateTime.Now.AddHours(24.0),
						HashCode = iranValidityResult.Data
					};
				}
				throw new CustomException(string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList()));
			}
		}
		throw new CustomException("خطا در ارسال اطلاعات ارسال رمز یکبارمصرف به سرور اعتبارسنجی ایران");
	}

	public async Task<IranValidityResult<string>> Validate(IranValidityValidateRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Token", request.Token);
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.PostAsync("b2b/api/request/" + request.HashCode + "/validate", new FormUrlEncodedContent(dictionary)))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				return JsonSerializer.Deserialize<IranValidityResult<string>>(json, options);
			}
		}
		throw new CustomException("خطا در ارسال درخواست اعتبارسنجی به سرور اعتبارسنجی ایران");
	}

	public async Task<IranValidityStatusResultData> GetReportStatus(IranValidityStatusRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		new Dictionary<string, string>();
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.GetAsync("b2b/api/request/" + request.HashCode + "/status"))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<IranValidityStatusResultData> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<IranValidityStatusResultData>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return iranValidityResult.Data;
				}
				throw new CustomException(string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList()));
			}
		}
		throw new CustomException("خطا در دریافت اطلاعات وضعیت درخواست از سرور اعتبارسنجی ایران");
	}

	public async Task<IranValidityStatusResultData> ReSendRequest(IranValidityReSendRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		Dictionary<string, string> nameValueCollection = new Dictionary<string, string>();
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.PostAsync("b2b/api/request/" + request.HashCode + "/reGenerateReport", new FormUrlEncodedContent(nameValueCollection)))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<IranValidityStatusResultData> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<IranValidityStatusResultData>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return iranValidityResult.Data;
				}
				throw new CustomException(string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList()));
			}
		}
		throw new CustomException("خطا در دریافت اطلاعات وضعیت درخواست از سرور اعتبارسنجی ایران");
	}

	public async Task<IranValidityPdfReportData> GetReportPDF(IranValidityStatusRequest request)
	{
		IranValidityTokenResultData iranValidityTokenResultData = await Token();
		new Dictionary<string, string>();
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", iranValidityTokenResultData.AccessToken);
		client.DefaultRequestHeaders.Add("x-apikey", iranValidityTokenResultData.ApiKey);
		using (HttpResponseMessage postResponse = await client.GetAsync("b2b/api/request/" + request.HashCode + "/pdfReport"))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<byte[]> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<byte[]>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return new IranValidityPdfReportData
					{
						Data = iranValidityResult.Data
					};
				}
				throw new CustomException(string.Join(',', iranValidityResult.Messages.Select((ResponseMessage x) => x.Message).ToList()));
			}
		}
		throw new CustomException("خطا در دریافت پی دی اف گزارش از سرور اعتبارسنجی ایران");
	}

	private async Task<IranValidityTokenResultData> Token()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Username", _iranValidityConfig.Username);
		dictionary.Add("Password", _iranValidityConfig.Password);
		using HttpClient client = new HttpClient();
		client.BaseAddress = new Uri((!string.IsNullOrWhiteSpace(_iranValidityConfig.BaseUrl)) ? (_iranValidityConfig.BaseUrl.TrimEnd('/') + "/") : BaseUrl);
		client.DefaultRequestHeaders.Accept.Clear();
		using (HttpResponseMessage postResponse = await client.PostAsync("Connect/Token", new FormUrlEncodedContent(dictionary)))
		{
			if (postResponse.StatusCode == HttpStatusCode.OK)
			{
				string json = await postResponse.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				IranValidityResult<IranValidityTokenResultData> iranValidityResult = JsonSerializer.Deserialize<IranValidityResult<IranValidityTokenResultData>>(json, options);
				if (!iranValidityResult.HasError)
				{
					return iranValidityResult.Data;
				}
			}
		}
		throw new CustomException("خطا در دریافت توکن");
	}
}
