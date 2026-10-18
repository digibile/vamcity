using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[DataContract(Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService")]
public class ConfirmPaymentResponseBody
{
    [DataMember(EmitDefaultValue = false, Order = 0)]
    public ClientConfirmResponseData ConfirmPaymentResult;

    public ConfirmPaymentResponseBody()
    {
    }

    public ConfirmPaymentResponseBody(ClientConfirmResponseData ConfirmPaymentResult)
    {
        this.ConfirmPaymentResult = ConfirmPaymentResult;
    }
}
