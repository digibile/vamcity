namespace LoanSystemAvizhe.Services.Models.Emzame;

public class EmzameContractCollaborator
{
	public string nationalCode { get; set; }

	public string fullName { get; set; }

	public string status { get; set; }

	public int order { get; set; }

	public string status_str => status switch
	{
		"reject" => "کاربر قرارداد را رد کرده است", 
		"accept" => "کاربر قرارداد را امضا کرده است", 
		"readyForSign" => "نوبت امضای کاربر است و کابر هنوز قرارداد را امضا یا رد نکرده است", 
		"waitForOtherSigners" => "نوبت امضای کاربر فرا نرسیده است", 
		"expired" => "کاربر قبل از زمان انقضای قرارداد آن را امضا نکرده است و قرارداد منقضی شده است", 
		_ => status, 
	};
}
