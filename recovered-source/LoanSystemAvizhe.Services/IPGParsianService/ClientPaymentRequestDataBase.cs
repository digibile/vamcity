using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientPaymentRequestDataBase", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
[KnownType(typeof(ClientSaleRequestData))]
[KnownType(typeof(ClientSaleDiscountRequestData))]
public class ClientPaymentRequestDataBase
{
    private string LoginAccountField;

    private long AmountField;

    private long OrderIdField;

    private string CallBackUrlField;

    private string AdditionalDataField;

    private string OriginatorField;

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

    [DataMember(IsRequired = true, Order = 2)]
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

    [DataMember(EmitDefaultValue = false, Order = 3)]
    public string CallBackUrl
    {
        get
        {
            return CallBackUrlField;
        }
        set
        {
            CallBackUrlField = value;
        }
    }

    [DataMember(EmitDefaultValue = false, Order = 4)]
    public string AdditionalData
    {
        get
        {
            return AdditionalDataField;
        }
        set
        {
            AdditionalDataField = value;
        }
    }

    [DataMember(EmitDefaultValue = false, Order = 5)]
    public string Originator
    {
        get
        {
            return OriginatorField;
        }
        set
        {
            OriginatorField = value;
        }
    }
}
