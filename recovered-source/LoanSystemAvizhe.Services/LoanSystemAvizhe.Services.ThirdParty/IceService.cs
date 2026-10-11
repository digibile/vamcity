using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using LoanSystem.Core.Exceptions;
using LoanSystem.Services.Models.Ice;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class IceService
{
	private readonly string BaseUrl = "https://{0}.icescoring.com/api/v1/";

	private IceConfig _iceConfig;

	private bool _developMode;

	public IceService(IceConfig iceConfig, bool developMode)
	{
		_iceConfig = iceConfig;
		_developMode = developMode;
	}

	public async Task<IceResult<IceAddIdentificationFromFirmResultData>> AddIdentificationFromFirm(IceAddIdentificationFromFirmRequest request)
	{
		string parameter = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(string.Format(BaseUrl, _developMode ? "SandboxFirm" : "apifirm"));
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
		new IceResult<IceAddIdentificationFromFirmResultData>();
		HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("identification/AddIdentificationFromFirm", request);
		if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
		{
			string str = await httpResponseMessage.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<IceResult<IceAddIdentificationFromFirmResultData>>(options: new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}, json: HttpUtility.UrlDecode(str));
		}
		throw new CustomException("خطا در ارسال اطلاعات اعتبارسنجی به سرور آیس");
	}

	public async Task<IceResult<IceGetStatusResultData>> GetReportStatus(IceGetStatusRequest request)
	{
		string parameter = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(string.Format(BaseUrl, _developMode ? "SandboxFirm" : "apifirm"));
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
		new IceResult<IceGetStatusResultData>();
		HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("identification/GetReportStatus?reportID=" + HttpUtility.UrlEncode(request.ReportID));
		if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
		{
			string json = await httpResponseMessage.Content.ReadAsStringAsync();
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			return JsonSerializer.Deserialize<IceResult<IceGetStatusResultData>>(json, options);
		}
		throw new CustomException("خطا در دریافت اطلاعات اعتبارسنجی از سرور آیس");
	}

	public async Task<IceResult<IceCancelIdentificationResultData>> CancelIdentification(IceCancelIdentificationRequest request)
	{
		string parameter = await Token();
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(string.Format(BaseUrl, _developMode ? "SandboxFirm" : "apifirm"));
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
		new IceResult<IceCancelIdentificationResultData>();
		HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("identification/CancelIdentification", request);
		if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
		{
			string json = await httpResponseMessage.Content.ReadAsStringAsync();
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			return JsonSerializer.Deserialize<IceResult<IceCancelIdentificationResultData>>(json, options);
		}
		throw new CustomException("خطا در ارسال اطلاعات اعتبارسنجی به سرور آیس");
	}

	private async Task<string> Token()
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(string.Format(BaseUrl, _developMode ? "SandboxFirm" : "apifirm"));
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("user/authenticate", new IceTokenRequest
		{
			UserName = _iceConfig.Username,
			Password = _iceConfig.Password
		});
		if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
		{
			string json = await httpResponseMessage.Content.ReadAsStringAsync();
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			IceResult<IceTokenResultData> iceResult = JsonSerializer.Deserialize<IceResult<IceTokenResultData>>(json, options);
			if (iceResult.State == 1)
			{
				return iceResult.Data.token;
			}
		}
		throw new CustomException("خطا در دریافت توکن");
	}
}
