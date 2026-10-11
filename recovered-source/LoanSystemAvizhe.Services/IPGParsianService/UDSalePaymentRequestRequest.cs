using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class UDSalePaymentRequestRequest
{
	[MessageBodyMember(Name = "UDSalePaymentRequest", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", Order = 0)]
	public UDSalePaymentRequestRequestBody Body;

	public UDSalePaymentRequestRequest()
	{
	}

	public UDSalePaymentRequestRequest(UDSalePaymentRequestRequestBody Body)
	{
		this.Body = Body;
	}
}
