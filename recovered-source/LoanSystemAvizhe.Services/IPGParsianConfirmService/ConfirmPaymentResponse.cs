using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentResponse
{
	[MessageBodyMember(Name = "ConfirmPaymentResponse", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentResponseBody Body;

	public ConfirmPaymentResponse()
	{
	}

	public ConfirmPaymentResponse(ConfirmPaymentResponseBody Body)
	{
		this.Body = Body;
	}
}
