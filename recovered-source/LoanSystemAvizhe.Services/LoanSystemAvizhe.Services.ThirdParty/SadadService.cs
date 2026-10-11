using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LoanSystemAvizhe.Services.Models.Sadad;
using Microsoft.Extensions.Logging;
using NikanBase.Services.Security;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class SadadService
{
	private readonly string BaseUrl = "https://sadad.shaparak.ir/api/v0/";

	private readonly ILogger<SadadService> _logger;

	private readonly IEncryptionService _encryptionService;

	public SadadService(ILogger<SadadService> logger, IEncryptionService encryptionService)
	{
		_logger = logger;
		_encryptionService = encryptionService;
	}

	public async Task<(string, SadadRequestResult)> Request(SadadPaymentRequest request)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		SadadRequestResult result = new SadadRequestResult();
		try
		{
			request.SignData = _encryptionService.EncryptTripleDes($"{request.TerminalId};{request.OrderId};{request.Amount}", request.MerchantKey);
			request.MerchantKey = "";
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("Request/PaymentRequest", request);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				result = (await httpResponseMessage.Content.ReadFromJsonAsync<SadadRequestResult>((JsonSerializerOptions?)null, default(CancellationToken))) ?? result;
				return ("https://sadad.shaparak.ir/Purchase/Index?token=" + result.Token, result);
			}
			return ("", result);
		}
		catch (Exception)
		{
			return ("", result);
		}
	}

	public async Task<SadadVerifyResult> Verify(SadadVerifyRequest verification)
	{
		HttpClient httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri(BaseUrl);
		httpClient.DefaultRequestHeaders.Accept.Clear();
		httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		SadadVerifyResult result = new SadadVerifyResult();
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(verification.Token);
			SymmetricAlgorithm? symmetricAlgorithm = SymmetricAlgorithm.Create("TripleDes");
			symmetricAlgorithm.Mode = CipherMode.ECB;
			symmetricAlgorithm.Padding = PaddingMode.PKCS7;
			string signData = Convert.ToBase64String(symmetricAlgorithm.CreateEncryptor(Convert.FromBase64String(verification.MerchantKey), new byte[8]).TransformFinalBlock(bytes, 0, bytes.Length));
			verification.SignData = signData;
			verification.MerchantKey = "";
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("Advice/Verify", verification);
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				result = (await httpResponseMessage.Content.ReadFromJsonAsync<SadadVerifyResult>((JsonSerializerOptions?)null, default(CancellationToken))) ?? result;
			}
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}
}
