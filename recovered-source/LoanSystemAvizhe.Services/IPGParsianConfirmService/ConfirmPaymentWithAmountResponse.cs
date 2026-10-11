using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentWithAmountResponse
{
	[MessageBodyMember(Name = "ConfirmPaymentWithAmountResponse", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentWithAmountResponseBody Body;

	public ConfirmPaymentWithAmountResponse()
	{
	}

	public ConfirmPaymentWithAmountResponse(ConfirmPaymentWithAmountResponseBody Body)
	{
		this.Body = Body;
	}
}
