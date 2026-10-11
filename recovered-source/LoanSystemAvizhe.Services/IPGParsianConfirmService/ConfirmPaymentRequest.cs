using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentRequest
{
	[MessageBodyMember(Name = "ConfirmPayment", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentRequestBody Body;

	public ConfirmPaymentRequest()
	{
	}

	public ConfirmPaymentRequest(ConfirmPaymentRequestBody Body)
	{
		this.Body = Body;
	}
}
