using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientPaymentResponseDataBase", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
[KnownType(typeof(ClientSaleResponseData))]
public class ClientPaymentResponseDataBase
{
	private long TokenField;

	private string MessageField;

	private short StatusField;

	[DataMember(IsRequired = true)]
	public long Token
	{
		get
		{
			return TokenField;
		}
		set
		{
			TokenField = value;
		}
	}

	[DataMember(EmitDefaultValue = false, Order = 1)]
	public string Message
	{
		get
		{
			return MessageField;
		}
		set
		{
			MessageField = value;
		}
	}

	[DataMember(IsRequired = true, Order = 2)]
	public short Status
	{
		get
		{
			return StatusField;
		}
		set
		{
			StatusField = value;
		}
	}
}
