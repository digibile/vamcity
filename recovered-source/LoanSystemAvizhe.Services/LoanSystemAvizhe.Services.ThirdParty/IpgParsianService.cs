using System;
using System.Threading.Tasks;
using IPGParsianConfirmService;
using IPGParsianService;
using LoanSystem.Services.Models.IPGParsian;

namespace LoanSystemAvizhe.Services.ThirdParty;

public class IpgParsianService
{
    private readonly string BaseUrl = "https://{0}.zarinpal.com/pg/v4/payment/";

    public async Task<(string, IPGParsianRequestResult)> Request(IPGParsianRequest request)
    {
        IPGParsianRequestResult result = new IPGParsianRequestResult();
        try
        {
            using (SaleServiceSoapClient SaleSvc = new SaleServiceSoapClient(SaleServiceSoapClient.EndpointConfiguration.SaleServiceSoap))
            {
                SalePaymentRequestResponse salePaymentRequestResponse = await SaleSvc.SalePaymentRequestAsync(new ClientSaleRequestData
                {
                    AdditionalData = request.AdditionalData,
                    Amount = request.Amount.GetValueOrDefault(),
                    CallBackUrl = request.CallBackUrl,
                    LoginAccount = request.LoginAccount,
                    OrderId = request.OrderId.GetValueOrDefault(),
                    Originator = request.Originator
                });
                result.Status = salePaymentRequestResponse.Body.SalePaymentRequestResult.Status;
                result.Message = salePaymentRequestResponse.Body.SalePaymentRequestResult.Message;
                result.Token = salePaymentRequestResponse.Body.SalePaymentRequestResult.Token;
                if (salePaymentRequestResponse.Body.SalePaymentRequestResult.Status == 0 && salePaymentRequestResponse.Body.SalePaymentRequestResult.Token > 0)
                {
                    return ("https://pec.shaparak.ir/NewIPG/?token=" + salePaymentRequestResponse.Body.SalePaymentRequestResult.Token, result);
                }
            }
            return ("", result);
        }
        catch (Exception)
        {
            return ("", result);
        }
    }

    public async Task<IPGParsianVerifyResult> Verify(IPGParsianVerify verification)
    {
        IPGParsianVerifyResult result = new IPGParsianVerifyResult();
        try
        {
            using (ConfirmServiceSoapClient confirmSvc = new ConfirmServiceSoapClient(ConfirmServiceSoapClient.EndpointConfiguration.ConfirmServiceSoap))
            {
                ConfirmPaymentResponse confirmPaymentResponse = await confirmSvc.ConfirmPaymentAsync(new ClientConfirmRequestData
                {
                    LoginAccount = verification.LoginAccount,
                    Token = verification.Token
                });
                result.CardNumberMasked = confirmPaymentResponse.Body.ConfirmPaymentResult.CardNumberMasked;
                result.RRN = confirmPaymentResponse.Body.ConfirmPaymentResult.RRN;
                result.Token = confirmPaymentResponse.Body.ConfirmPaymentResult.Token;
                result.Status = confirmPaymentResponse.Body.ConfirmPaymentResult.Status;
            }
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }
}
