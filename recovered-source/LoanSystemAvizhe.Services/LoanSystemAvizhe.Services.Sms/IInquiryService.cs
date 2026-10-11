using System.Collections.Generic;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;

namespace LoanSystemAvizhe.Services.Sms;

public interface IInquiryService
{
	ResponseInquiry<ShahkarResult> Shahkar(BaseSendInquiry sendInquiry);

	ResponseInquiry<ShahkarResult> Shahkar3(BaseSendInquiry sendInquiry);

	ResponseInquiry<SanaResult> Sana(BaseSendInquiry sendInquiry);

	ResponseInquiry<UnpaidChequeResult> UnpaidCheque(BaseSendInquiry sendInquiry);

	ResponseInquiry<PersonInfoResult> PersonInfo(BaseSendInquiry sendInquiry);

	ResponseInquiry<IbanInfoResult> IbanInfo(BaseSendInquiry sendInquiry);

	ResponseInquiry<BankCardInfoResult> BankCardInfo(BaseSendInquiry sendInquiry);

	ResponseInquiry<PostalAddressResult> PostalAddress(BaseSendInquiry sendInquiry);

	ResponseInquiry<List<ActivePlatesResult>> ActivePlates(BaseSendInquiry sendInquiry);

	ResponseInquiry<CardMatchResult> CardMatch(BaseSendInquiry sendInquiry);

	ResponseInquiry<IbanMatchResult> IbanMatch(BaseSendInquiry sendInquiry);

	ResponseInquiry<ChequeColorResult> ChequeColor(BaseSendInquiry sendInquiry);

	ResponseInquiry<ChequeInfoResult> ChequeInfo(BaseSendInquiry sendInquiry);

	ResponseInquiry<ActiveLoansResult> ActiveLoans(BaseSendInquiry sendInquiry);

	ResponseInquiry<MilitaryStatusResult> MilitaryStatus(BaseSendInquiry sendInquiry);

	ResponseInquiry<CreditResult> GetCredit();
}
