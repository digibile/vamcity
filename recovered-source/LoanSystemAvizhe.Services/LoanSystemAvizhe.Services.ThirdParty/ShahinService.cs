using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystemAvizhe.Services.Models.Shahin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shahin.Helper;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class ShahinService
{
	private readonly ShahinConfig _shahinConfig;

	private string _baseUrl;

	private string _contentRootPath;

	private readonly ILogger<ShahinService> _logger;

	public ShahinService(ILogger<ShahinService> logger, IOptions<ShahinConfig> shahinConfig)
	{
		_shahinConfig = shahinConfig.Value;
		_baseUrl = "https://" + _shahinConfig.Ip + ":" + _shahinConfig.Port;
		_logger = logger;
	}

	public async Task<ShahinTransferValidationResult> ShahinTransferValidattion(ShahinTransferRequestModel request)
	{
		ShahinTokenResultData shahinTokenResultData = await Token();
		ShahinTransferValidationResult shahinTransferValidationResult = new ShahinTransferValidationResult();
		if (request.transferType == "SATNA" && string.IsNullOrEmpty(request.destinationAccountName))
		{
			shahinTransferValidationResult.respObject.reason = new List<string> { "در انتقال ساتنا باید نام صاحب حساب مقصد پر شده باشد." };
			return shahinTransferValidationResult;
		}
		if (!shahinTokenResultData.Accounts.Contains(request.sourceAccount))
		{
			shahinTransferValidationResult.respObject.reason = new List<string> { "حساب مبدا انتخاب شده مجاز نمی باشد." };
			return shahinTransferValidationResult;
		}
		try
		{
			string text = Guid.NewGuid().ToString();
			string text2 = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).Ticks.ToString().Substring(0, 13);
			HttpClientHandler handler = CreateHttpClientHandler();
			string text3 = JsonConvert.SerializeObject(new JsonObj_transfer_validation
			{
				bank = "BSI",
				nationalCode = shahinTokenResultData.User_Name,
				sourceAccount = request.sourceAccount,
				destinationBank = request.destinationBank,
				oneWithdraw = "",
				correlationID = "",
				destinationAccountNumber = request.destinationAccountNumber,
				amount = request.amount.ToString(),
				babat = request.babat,
				documentID = request.documentID,
				paymentID = request.paymentID,
				transferType = request.transferType,
				description = request.depositDescription,
				toNationalID = "",
				toPostalID = ""
			});
			string text4 = "v0.3/obh/api/pisp/transfer-validation";
			string value = PrepareObhSignature(text4, text2, text, text3);
			HttpClient httpClient = new HttpClient(handler);
			httpClient.BaseAddress = new Uri(_baseUrl);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", shahinTokenResultData.Access_Token);
			httpClient.DefaultRequestHeaders.Add("X-Obh-timestamp", text2);
			httpClient.DefaultRequestHeaders.Add("X-Obh-uuid", text);
			httpClient.DefaultRequestHeaders.Add("X-Obh-signature", value);
			StringContent content = new StringContent(text3, Encoding.UTF8, "application/json");
			Task<HttpResponseMessage> task = httpClient.PostAsync(text4, content);
			string result = task.Result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("ShahinTransferValidattion PostResponse Content:" + result);
			if (task.Result.IsSuccessStatusCode || task.Result.StatusCode == HttpStatusCode.BadRequest)
			{
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				shahinTransferValidationResult = System.Text.Json.JsonSerializer.Deserialize<ShahinTransferValidationResult>(result, options);
				if (shahinTransferValidationResult.respObject.destinationAccountFullName != request.destinationAccountName)
				{
					shahinTransferValidationResult.transactionState = "";
					shahinTransferValidationResult.respObject.reason = new List<string> { "نام شخص دریافت کننده با اطلاعات ارسالی مطابقت ندارد." };
					return shahinTransferValidationResult;
				}
				return shahinTransferValidationResult;
			}
			shahinTransferValidationResult.respObject.reason = new List<string> { "خطا در ارتباط با سرور شاهین." };
			return shahinTransferValidationResult;
		}
		catch (Exception ex)
		{
			shahinTransferValidationResult.respObject.reason = new List<string> { "خطا در ارتباط با سرور شاهین: " + ex.Message };
			return shahinTransferValidationResult;
		}
	}

	public async Task<ShahinIbanInfoResult> ShahinIbanInfo(ShahinIbanInfoRequestModel request)
	{
		ShahinTokenResultData shahinTokenResultData = await Token();
		ShahinIbanInfoResult shahinIbanInfoResult = new ShahinIbanInfoResult();
		try
		{
			string text = Guid.NewGuid().ToString();
			string text2 = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).Ticks.ToString().Substring(0, 13);
			HttpClientHandler handler = CreateHttpClientHandler();
			request.bank = "BSI";
			request.nationalCode = shahinTokenResultData.User_Name;
			string text3 = JsonConvert.SerializeObject(request);
			string text4 = "v0.3/obh/api/aisp/get-iban-info";
			string value = PrepareObhSignature(text4, text2, text, text3);
			HttpClient httpClient = new HttpClient(handler);
			httpClient.BaseAddress = new Uri(_baseUrl);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", shahinTokenResultData.Access_Token);
			httpClient.DefaultRequestHeaders.Add("X-Obh-timestamp", text2);
			httpClient.DefaultRequestHeaders.Add("X-Obh-uuid", text);
			httpClient.DefaultRequestHeaders.Add("X-Obh-signature", value);
			StringContent content = new StringContent(text3, Encoding.UTF8, "application/json");
			Task<HttpResponseMessage> task = httpClient.PostAsync(text4, content);
			string result = task.Result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("ShahinIbanInfo PostResponse Content:" + result);
			if (task.Result.IsSuccessStatusCode || task.Result.StatusCode == HttpStatusCode.BadRequest)
			{
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				shahinIbanInfoResult = System.Text.Json.JsonSerializer.Deserialize<ShahinIbanInfoResult>(result, options);
				return shahinIbanInfoResult;
			}
			shahinIbanInfoResult.message = "خطا در ارتباط با سرور شاهین.";
			return shahinIbanInfoResult;
		}
		catch (Exception ex)
		{
			shahinIbanInfoResult.message = "خطا در ارتباط با سرور شاهین: " + ex.Message;
			return shahinIbanInfoResult;
		}
	}

	public async Task<ShahinTransferResult> Transfer(ShahinTransferRequestModel request)
	{
		ShahinTokenResultData shahinTokenResultData = await Token();
		ShahinTransferResult result = new ShahinTransferResult();
		if (request.transferType == "SATNA" && string.IsNullOrEmpty(request.destinationAccountName))
		{
			result.message = "در انتقال ساتنا باید نام صاحب حساب مقصد پر شده باشد.";
			return result;
		}
		if (!shahinTokenResultData.Accounts.Contains(request.sourceAccount))
		{
			result.message = "حساب مبدا انتخاب شده مجاز نمی باشد.";
			return result;
		}
		try
		{
			string text = Guid.NewGuid().ToString();
			string text2 = (DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).Ticks.ToString().Substring(0, 13);
			HttpClientHandler handler = CreateHttpClientHandler();
			string text3 = JsonConvert.SerializeObject(new JsonObj_transfer
			{
				bank = "BSI",
				nationalCode = shahinTokenResultData.User_Name,
				sourceAccount = request.sourceAccount,
				destinationAccountNumber = request.destinationAccountNumber,
				destinationAccountName = request.destinationAccountName,
				destinationBank = request.destinationBank,
				amount = request.amount.ToString(),
				transferType = request.transferType,
				babat = request.babat,
				paymentID = request.paymentID,
				documentID = request.documentID,
				transferID = request.transferID,
				depositDescription = request.depositDescription,
				withdrawDescription = request.withdrawDescription,
				smsPass = ""
			});
			string text4 = "v0.3/obh/api/pisp/transfer";
			string value = PrepareObhSignature(text4, text2, text, text3);
			HttpClient httpClient = new HttpClient(handler);
			httpClient.BaseAddress = new Uri(_baseUrl);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", shahinTokenResultData.Access_Token);
			httpClient.DefaultRequestHeaders.Add("X-Obh-timestamp", text2);
			httpClient.DefaultRequestHeaders.Add("X-Obh-uuid", text);
			httpClient.DefaultRequestHeaders.Add("X-Obh-signature", value);
			StringContent content = new StringContent(text3, Encoding.UTF8, "application/json");
			Task<HttpResponseMessage> task = httpClient.PostAsync(text4, content);
			string result2 = task.Result.Content.ReadAsStringAsync().Result;
			_logger.LogInformation("ShahinTransfer PostResponse Content:" + result2);
			if (task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.BadRequest)
			{
				string json = await task.Result.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				result = System.Text.Json.JsonSerializer.Deserialize<ShahinTransferResult>(json, options);
				return result;
			}
			result.message = "خطا در ارتباط با سرور شاهین.";
			return result;
		}
		catch (Exception ex)
		{
			result.message = "خطا در ارتباط با سرور شاهین: " + ex.Message;
			return result;
		}
	}

	private async Task<ShahinTokenResultData> Token()
	{
		_logger.LogInformation("Start Token");
		string username = _shahinConfig.Username;
		string password = _shahinConfig.Password;
		try
		{
			ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true));
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			string fileName = Path.Combine(_contentRootPath, "App_Data", "certs", "orgCertificate.pfx");
			HttpClientHandler httpClientHandler = new HttpClientHandler
			{
				SslProtocols = SslProtocols.Tls12
			};
			httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
			X509Certificate2 value = new X509Certificate2(fileName, "123!@#qweQWE", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
			httpClientHandler.ClientCertificates.Add(value);
			httpClientHandler.ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
			string text = "https://" + _shahinConfig.Ip + ":" + _shahinConfig.TokenPort + "/v0.3/obh/oauth/token";
			HttpClient httpClient = new HttpClient(httpClientHandler);
			httpClient.BaseAddress = new Uri(text);
			string s = username + ":" + password;
			string parameter = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", parameter);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			string content = "grant_type=client_credentials&bank=BSI";
			HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(text, new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded"));
			if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
			{
				string json = await httpResponseMessage.Content.ReadAsStringAsync();
				JsonSerializerOptions options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};
				ShahinTokenResultData shahinTokenResultData = System.Text.Json.JsonSerializer.Deserialize<ShahinTokenResultData>(json, options);
				if (!string.IsNullOrWhiteSpace(shahinTokenResultData.Access_Token))
				{
					return shahinTokenResultData;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogInformation("Shahin Token ex.innerExceptoin: " + ex.InnerException);
			throw;
		}
		throw new CustomException("خطا در دریافت توکن");
	}

	private HttpClientHandler CreateHttpClientHandler()
	{
		ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true));
		ServicePointManager.Expect100Continue = true;
		ServicePointManager.DefaultConnectionLimit = 9999;
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		string fileName = Path.Combine(_contentRootPath, "App_Data", "certs", "orgCertificate.pfx");
		HttpClientHandler httpClientHandler = new HttpClientHandler();
		httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
		httpClientHandler.SslProtocols = SslProtocols.Tls12;
		httpClientHandler.ClientCertificates.Add(new X509Certificate2(fileName, "123!@#qweQWE"));
		httpClientHandler.ServerCertificateCustomValidationCallback = (HttpRequestMessage sender, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true;
		return httpClientHandler;
	}

	public string PrepareObhSignature(string uri, string timestamp, string uuid, string jsonBody)
	{
		string text = new HelperShahin().Sign("POST", uri, jsonBody, _shahinConfig.Username, _shahinConfig.Password, timestamp, uuid);
		return "OBH1-HMAC-SHA256;SignedHeaders=X-Obh-uuid,X-Obh-timestamp;signature=" + text;
	}

	public void SetWebHostRoot(string contentRootPath)
	{
		_contentRootPath = contentRootPath;
	}
}
