using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class ConfirmPaymentWithAddDataRequest
{
	[MessageBodyMember(Name = "ConfirmPaymentWithAddData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService", Order = 0)]
	public ConfirmPaymentWithAddDataRequestBody Body;

	public ConfirmPaymentWithAddDataRequest()
	{
	}

	public ConfirmPaymentWithAddDataRequest(ConfirmPaymentWithAddDataRequestBody Body)
	{
		this.Body = Body;
	}
}
