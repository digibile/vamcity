using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class SalePaymentWithIdResponse
{
	[MessageBodyMember(Name = "SalePaymentWithIdResponse", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", Order = 0)]
	public SalePaymentWithIdResponseBody Body;

	public SalePaymentWithIdResponse()
	{
	}

	public SalePaymentWithIdResponse(SalePaymentWithIdResponseBody Body)
	{
		this.Body = Body;
	}
}
