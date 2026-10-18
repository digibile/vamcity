namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

internal class BaleMessageData
{
    public string request_id { get; set; }

    public long bot_id { get; set; }

    public string phone_number { get; set; }

    public MessageDataModel message_data { get; set; }
}
