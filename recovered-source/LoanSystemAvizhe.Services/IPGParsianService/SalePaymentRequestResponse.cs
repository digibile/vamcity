using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[MessageContract(IsWrapped = false)]
public class SalePaymentRequestResponse
{
    [MessageBodyMember(Name = "SalePaymentRequestResponse", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService", Order = 0)]
    public SalePaymentRequestResponseBody Body;

    public SalePaymentRequestResponse()
    {
    }

    public SalePaymentRequestResponse(SalePaymentRequestResponseBody Body)
    {
        this.Body = Body;
    }
}
