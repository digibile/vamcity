using System.Collections.Generic;
using LoanSystemAvizhe.Services.Email;
using LoanSystemAvizhe.Services.Models.Email;

namespace LoanSystemAvizhe.Services.Base;

public interface IEmailService
{
    void SetEmailConfig(EmailConfig emailConfig);

    string Send(EmailMessage emailMessage);

    List<EmailMessage> ReceiveEmail(int maxCount = 100);
}
