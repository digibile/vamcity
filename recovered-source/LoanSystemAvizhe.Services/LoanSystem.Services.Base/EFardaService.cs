using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Services.Models.EFarda;
using LoanSystem.Services.Models.Sep;
using Microsoft.Extensions.Options;

namespace LoanSystem.Services.Base;

public class EFardaService
{
	private readonly EFardaConfig _EFardaConfig;

	private readonly string BaseUrl = "https://refund.efarda.ir/refund/";

	public EFardaService(IOptions<EFardaConfig> EFardaConfig)
	{
		_EFardaConfig = EFardaConfig.Value;
	}

	public async Task<EFardaResult> Request(EFardaRequest request)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		httpClient.DefaultRequestHeaders.Add("username", _EFardaConfig.Username);
		httpClient.DefaultRequestHeaders.Add("password", _EFardaConfig.Password);
		EFardaResult result = new EFardaResult();
		try
		{
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/v1/refundWithIban", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = JsonSerializer.Deserialize<EFardaResult>(json, options);
				return result;
			}
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	private string Encrypt(string message, string privateKey)
	{
		byte[] inArray;
		using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			inArray = rSACryptoServiceProvider.SignData(bytes, new SHA256CryptoServiceProvider());
		}
		return Convert.ToBase64String(inArray);
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
