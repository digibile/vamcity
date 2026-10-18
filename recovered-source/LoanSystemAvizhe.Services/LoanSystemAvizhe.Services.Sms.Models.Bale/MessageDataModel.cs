namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class MessageDataModel
{
    public OptMessageModel otp_message { get; private set; }

    public MessageModel message { get; private set; }

    public MessageDataModel(string otp)
    {
        otp_message = new OptMessageModel(otp);
    }

    public MessageDataModel(MessageModel msg)
    {
        message = msg;
    }
}
