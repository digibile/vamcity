using LoanSystemAvizhe.Services.Sms.Models.IPPanel;
using LoanSystemAvizhe.Services.Sms.Models.KaveNegar;
using LoanSystemAvizhe.Services.Sms.Models.MelliPayamak;
using LoanSystemAvizhe.Services.Sms.Models.WebOne;

namespace LoanSystemAvizhe.Services.Sms;

public interface IPayamakService
{
    ResponsePayamak SendByPatternWebOne(SendWebOnePattern sendPayamak, string patternName);

    ResponsePayamak SendByPatternIPPanel(SendIPPanelPattern sendPayamak, string patternName);

    ResponsePayamak SendByLookupKaveNegar(SendKaveNegar sendPayamak, string templateName);

    ResponsePayamak SendByBaseNumber(BaseSendPayamak sendPayamak, int bodyId);

    ResponsePayamak Send(BaseSendPayamak sendPayamak);

    ResponsePayamak CheckDelivery(string recId);

    ResponsePayamak GetBasePrice(BaseSendPayamak sendMelliPayamak);

    ResponsePayamak GetCredit();
}
