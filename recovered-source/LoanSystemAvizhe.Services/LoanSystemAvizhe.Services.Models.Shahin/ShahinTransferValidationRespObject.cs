using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoanSystemAvizhe.Services.Models.Shahin;

public class ShahinTransferValidationRespObject
{
    public string destinationAccountFullName { get; set; }

    public string destinationAccountNumber { get; set; }

    public string message { get; set; }

    public List<ShahinTransferValidationRespObjectSubErrors> subErrors { get; set; }

    public List<string> reason { get; set; }

    public string reasonText
    {
        get
        {
            List<string> list = reason;
            if (list != null && list.Any())
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string item in reason)
                {
                    switch (item)
                    {
                        case "INSUFFICIENT_BALANCE":
                            stringBuilder.Append("موجودی ناکافی,");
                            break;
                        case "SOURCE_ACCOUNT_CLOSED":
                            stringBuilder.Append("حساب مبدا مسدود یا نادرست است,");
                            break;
                        case "DESTINATION_ACCOUNT_CLOSED":
                            stringBuilder.Append("حساب مقصد مسدود یا نادرست است,");
                            break;
                        default:
                            stringBuilder.Append(item + ",");
                            break;
                    }
                }
                return stringBuilder.ToString().TrimEnd(',');
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                StringBuilder stringBuilder2 = new StringBuilder();
                stringBuilder2.Append(message);
                if (subErrors != null)
                {
                    foreach (ShahinTransferValidationRespObjectSubErrors subError in subErrors)
                    {
                        stringBuilder2.Append(" [" + subError.field + ":" + subError.rejectedValue + "],");
                    }
                }
                return stringBuilder2.ToString().TrimEnd(',');
            }
            return "";
        }
    }

    public string transferId { get; set; }

    public string transferState { get; set; }

    public string transferType { get; set; }
}
