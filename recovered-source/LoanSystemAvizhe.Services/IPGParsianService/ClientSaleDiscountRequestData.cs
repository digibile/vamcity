using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace IPGParsianService;

[DebuggerStepThrough]
[GeneratedCode("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
[DataContract(Name = "ClientSaleDiscountRequestData", Namespace = "https://pec.Shaparak.ir/NewIPGServices/Sale/SaleService")]
public class ClientSaleDiscountRequestData : ClientSaleRequestData
{
    private Product[] DiscountProductField;

    [DataMember(EmitDefaultValue = false)]
    public Product[] DiscountProduct
    {
        get
        {
            return DiscountProductField;
        }
        set
        {
            DiscountProductField = value;
        }
    }
}
