using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientConfirmResponseData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService")]
[KnownType(typeof(ClientConfirmResponseDataAddData))]
public class ClientConfirmResponseData
{
	private short StatusField;

	private string CardNumberMaskedField;

	private long RRNField;

	private long TokenField;

	[DataMember(IsRequired = true)]
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

	[DataMember(EmitDefaultValue = false, Order = 1)]
	public string CardNumberMasked
	{
		get
		{
			return CardNumberMaskedField;
		}
		set
		{
			CardNumberMaskedField = value;
		}
	}

	[DataMember(IsRequired = true, Order = 2)]
	public long RRN
	{
		get
		{
			return RRNField;
		}
		set
		{
			RRNField = value;
		}
	}

	[DataMember(IsRequired = true, Order = 3)]
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
}
