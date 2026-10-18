using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Services.Models.NextPay;
using RestSharp;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class NextPayService
{
    private readonly string BaseUrl = "https://nextpay.org/nx/gateway";

    public async Task<(string, NextPayRequestResult)> Request(NextPayRequest request)
    {
        RestClient client = new RestClient(new Uri(BaseUrl + "/token"));
        RestRequest request2 = new RestRequest(new Uri(BaseUrl + "/token"), Method.Post);
        request2.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        request2.AddParameter("api_key", request.api_key);
        request2.AddParameter("amount", request.amount);
        request2.AddParameter("order_id", request.order_id.Replace("-", ""));
        request2.AddParameter("callback_uri", request.callback_url);
        if (!string.IsNullOrEmpty(request.customer_phone))
        {
            request2.AddParameter("customer_phone", request.customer_phone);
        }
        NextPayRequestResult nextPayRequestResult = new NextPayRequestResult();
        try
        {
            RestResponse restResponse = client.Execute(request2);
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                string? content = restResponse.Content;
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                nextPayRequestResult = JsonSerializer.Deserialize<NextPayRequestResult>(content, options);
                return (BaseUrl + "/payment/" + nextPayRequestResult.trans_id, nextPayRequestResult);
            }
            return ("", nextPayRequestResult);
        }
        catch (Exception)
        {
            return ("", nextPayRequestResult);
        }
    }

    public async Task<NextPayVerifyResult> Verify(NextPayVerify verification)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
        httpClient.DefaultRequestHeaders.Add("api_key", verification.api_key);
        httpClient.DefaultRequestHeaders.Add("amount", verification.amount.ToString());
        httpClient.DefaultRequestHeaders.Add("trans_id", verification.trans_id);
        NextPayVerifyResult result = new NextPayVerifyResult();
        try
        {
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/verify", verification);
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string json = await httpResponseMessage.Content.ReadAsStringAsync();
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                result = JsonSerializer.Deserialize<NextPayVerifyResult>(json, options);
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }
}
