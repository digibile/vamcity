using System.CodeDom.Compiler;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IPGParsianConfirmService;

[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[ServiceContract(Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", ConfigurationName = "IPGParsianConfirmService.ConfirmServiceSoap")]
public interface ConfirmServiceSoap
{
    [OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService/ConfirmPayment", ReplyAction = "*")]
    Task<ConfirmPaymentResponse> ConfirmPaymentAsync(ConfirmPaymentRequest request);

    [OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService/ConfirmPaymentWithAddData", ReplyAction = "*")]
    Task<ConfirmPaymentWithAddDataResponse> ConfirmPaymentWithAddDataAsync(ConfirmPaymentWithAddDataRequest request);

    [OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService/ConfirmPaymentWithAmount", ReplyAction = "*")]
    Task<ConfirmPaymentWithAmountResponse> ConfirmPaymentWithAmountAsync(ConfirmPaymentWithAmountRequest request);
}
