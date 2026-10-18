using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "Product", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
public class Product
{
    private int PGroupIdField;

    private long AmountField;

    [DataMember(IsRequired = true)]
    public int PGroupId
    {
        get
        {
            return PGroupIdField;
        }
        set
        {
            PGroupIdField = value;
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
