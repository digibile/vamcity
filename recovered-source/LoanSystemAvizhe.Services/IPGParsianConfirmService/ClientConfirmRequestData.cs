using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientConfirmRequestData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService")]
[KnownType(typeof(ClientConfirmWithAmountRequestData))]
public class ClientConfirmRequestData
{
	private string LoginAccountField;

	private long TokenField;

	[DataMember(EmitDefaultValue = false)]
	public string LoginAccount
	{
		get
		{
			return LoginAccountField;
		}
		set
		{
			LoginAccountField = value;
		}
	}

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
}
