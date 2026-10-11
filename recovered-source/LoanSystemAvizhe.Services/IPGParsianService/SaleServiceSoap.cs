using System.CodeDom.Compiler;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IPGParsianService;

[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[ServiceContract(Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", ConfigurationName = "IPGParsianService.SaleServiceSoap")]
public interface SaleServiceSoap
{
	[OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/SalePaymentRequest", ReplyAction = "*")]
	Task<SalePaymentRequestResponse> SalePaymentRequestAsync(SalePaymentRequestRequest request);

	[OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/SalePaymentWithId", ReplyAction = "*")]
	Task<SalePaymentWithIdResponse> SalePaymentWithIdAsync(SalePaymentWithIdRequest request);

	[OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/UDSalePaymentRequest", ReplyAction = "*")]
	Task<UDSalePaymentRequestResponse> UDSalePaymentRequestAsync(UDSalePaymentRequestRequest request);

	[OperationContract(Action = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService/SalePaymentWithDiscount", ReplyAction = "*")]
	Task<SalePaymentWithDiscountResponse> SalePaymentWithDiscountAsync(SalePaymentWithDiscountRequest request);
}
