using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class SalePaymentWithDiscountRequest
{
	[MessageBodyMember(Name = "SalePaymentWithDiscount", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", Order = 0)]
	public SalePaymentWithDiscountRequestBody Body;

	public SalePaymentWithDiscountRequest()
	{
	}

	public SalePaymentWithDiscountRequest(SalePaymentWithDiscountRequestBody Body)
	{
		this.Body = Body;
	}
}
