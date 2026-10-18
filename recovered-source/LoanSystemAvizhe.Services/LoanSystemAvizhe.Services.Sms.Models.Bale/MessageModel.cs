namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class MessageModel
{
    public string text { get; private set; }

    public string copy_text { get; private set; }

    public MessageModel(string txt)
    {
        text = txt;
        copy_text = txt;
    }
}
