namespace LoanSystemAvizhe.Services.Sms.Models.Bale;

public class SendBaleErrorInfo
{
	public string phone_number { get; set; }

	public int code { get; set; }

	public string description => code switch
	{
		2 => "خطای داخلی سرور بله", 
		3 => "بیش از حد مجاز پیام ارسال شده", 
		4 => "ورودی JSON نامعتبر", 
		8 => "شماره اشتباه", 
		17 => "کاربر اکانت بله ندارد", 
		20 => "اعتبار کافی وجود ندارد", 
		_ => "خطای نامشخص", 
	};
}
