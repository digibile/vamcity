using System.Collections.Generic;

namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class SendBaleMessageResult
{
    public string message_id { get; set; }

    public bool success => error_data == null;

    public List<SendBaleErrorInfo> error_data { get; set; }
}
