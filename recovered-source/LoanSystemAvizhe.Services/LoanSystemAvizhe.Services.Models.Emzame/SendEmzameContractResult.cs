using System;
using LoanSystem.Core;

namespace LoanSystemAvizhe.Services.Models.Emzame;

public class SendEmzameContractResult : SendEmzameErrorResult
{
	public string id { get; set; }

	public string status { get; set; }

	public string[] signedFiles { get; set; }

	public EmzameContractCollaborator[] collaborator { get; set; }

	public string lastSignedFileId { get; set; }

	public string _id { get; set; }

	public string title { get; set; }

	public string userId { get; set; }

	public string businessId { get; set; }

	public string description { get; set; }

	public string provider { get; set; }

	public string signType { get; set; }

	public string staticFileId { get; set; }

	public bool isElectronicSignatureRequired { get; set; }

	public bool isSignatureDescriptionRequired { get; set; }

	public bool shouldEveryPageHaveSignatureImage { get; set; }

	public string contractType { get; set; }

	public long createdDate { get; set; }

	public long changedDate { get; set; }

	public long statusDescription { get; set; }

	public long expireTime { get; set; }

	public long generatedPdfId { get; set; }

	public bool isBlankPageRequire { get; set; }

	public string status_str => status switch
	{
		"reject" => "قرارداد توسط یکی از امضا کنندگان رد شده است", 
		"accept" => "قرارداد امضا شده است", 
		"readyForSign" => "یک یا چند نفر از امضا کنندگان هنوز قرارداد را امضا نکرده اند", 
		"expired" => "زمان امضای قرارداد گذشته است و قرارداد منقضی شده است", 
		_ => status, 
	};

	public PersianDate changedDate_persian => DateTimeOffset.FromUnixTimeMilliseconds(changedDate).DateTime.ToPersianDateObject();
}
