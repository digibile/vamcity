using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[EditorBrowsable(EditorBrowsableState.Advanced)]
[DataContract(Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
public class UDSalePaymentRequestRequestBody
{
	[DataMember(EmitDefaultValue = false, Order = 0)]
	public ClientSaleRequestData requestData;

	public UDSalePaymentRequestRequestBody()
	{
	}

	public UDSalePaymentRequestRequestBody(ClientSaleRequestData requestData)
	{
		this.requestData = requestData;
	}
}
