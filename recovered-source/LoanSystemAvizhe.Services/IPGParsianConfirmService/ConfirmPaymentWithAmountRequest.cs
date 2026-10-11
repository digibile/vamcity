using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentWithAmountRequest
{
	[MessageBodyMember(Name = "ConfirmPaymentWithAmount", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentWithAmountRequestBody Body;

	public ConfirmPaymentWithAmountRequest()
	{
	}

	public ConfirmPaymentWithAmountRequest(ConfirmPaymentWithAmountRequestBody Body)
	{
		this.Body = Body;
	}
}
