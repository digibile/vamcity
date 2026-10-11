using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentWithAddDataResponse
{
	[MessageBodyMember(Name = "ConfirmPaymentWithAddDataResponse", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentWithAddDataResponseBody Body;

	public ConfirmPaymentWithAddDataResponse()
	{
	}

	public ConfirmPaymentWithAddDataResponse(ConfirmPaymentWithAddDataResponseBody Body)
	{
		this.Body = Body;
	}
}
