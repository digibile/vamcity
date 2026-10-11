using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientConfirmResponseDataAddData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService")]
public class ClientConfirmResponseDataAddData : ClientConfirmResponseData
{
	private string AddDataField;

	[DataMember(EmitDefaultValue = false)]
	public string AddData
	{
		get
		{
			return AddDataField;
		}
		set
		{
			AddDataField = value;
		}
	}
}
