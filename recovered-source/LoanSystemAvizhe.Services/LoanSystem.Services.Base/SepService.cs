using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Services.Models.Sep;

namespace LoanSystem.Services.Base;

public class SepService
{
	private readonly string BaseUrl = "https://sep.shaparak.ir";

	public async Task<(string, SepRequestResult)> Request(SepRequest request)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		SepRequestResult result = new SepRequestResult();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/onlinepg/onlinepg", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = (result.ErrorDesc = await httpResponseMessage.Content.ReadAsStringAsync());
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<SepRequestResult>(json, options);
				return (BaseUrl + "/OnlinePG/OnlinePG", result);
			}
			return ("", result);
		}
		catch (Exception)
		{
			return ("", result);
		}
	}

	public async Task<SepVerifyResult> Verify(SepVerify verification)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		SepVerifyResult result = new SepVerifyResult();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/verifyTxnRandomSessionkey/ipg/VerifyTransaction", verification);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<SepVerifyResult>(json, options);
			}
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}
}
