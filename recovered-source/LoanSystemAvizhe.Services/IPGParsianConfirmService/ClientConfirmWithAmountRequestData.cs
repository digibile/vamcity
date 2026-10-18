using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianConfirmService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientConfirmWithAmountRequestData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Confirm/ConfirmService")]
public class ClientConfirmWithAmountRequestData : ClientConfirmRequestData
{
    private long OrderIdField;

    private long AmountField;

    [DataMember(IsRequired = true)]
    public long OrderId
    {
        get
        {
            return OrderIdField;
        }
        set
        {
            OrderIdField = value;
        }
    }

    [DataMember(IsRequired = true, Order = 1)]
    public long Amount
    {
        get
        {
            return AmountField;
        }
        set
        {
            AmountField = value;
        }
    }
}
