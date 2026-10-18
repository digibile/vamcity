namespace LoanSystemAvizhe.Data.Enums;

public enum PaymentSteps
{
    Start,
    InvokePayment,
    ResponsedInvokePayment,
    RedirectToGateway,
    InvokePaymentVerification,
    ResponsedInvokePaymentVerification,
    Finish
}
