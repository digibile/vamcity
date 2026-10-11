using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[DataContract(Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
public class SalePaymentWithDiscountResponseBody
{
	[DataMember(EmitDefaultValue = false, Order = 0)]
	public ClientSaleResponseData SalePaymentWithDiscountResult;

	public SalePaymentWithDiscountResponseBody()
	{
	}

	public SalePaymentWithDiscountResponseBody(ClientSaleResponseData SalePaymentWithDiscountResult)
	{
		this.SalePaymentWithDiscountResult = SalePaymentWithDiscountResult;
	}
}
