using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystemAvizhe.Services.Email;
using LoanSystemAvizhe.Services.Models.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace LoanSystemAvizhe.Services.Base;

public class EmailService : IEmailService
{
    private EmailConfig _emailConfig;

    private readonly IServiceScopeFactory _scopeFactory;

    public EmailService(IOptions<EmailConfig> emailConfig, IServiceScopeFactory scopeFactory)
    {
        _emailConfig = emailConfig.Value;
        _scopeFactory = scopeFactory;
    }

    public void SetEmailConfig(EmailConfig emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public List<EmailMessage> ReceiveEmail(int maxCount = 100)
    {
        throw new NotImplementedException();
    }

    public string Send(EmailMessage emailMessage)
    {
        try
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailConfig.Name, _emailConfig.SmtpUsername));
            mimeMessage.To.Add(new MailboxAddress(emailMessage.ToAddress.Name, emailMessage.ToAddress.Address));
            mimeMessage.Subject = emailMessage.Subject;
            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };
            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Connect(_emailConfig.SmtpHost, _emailConfig.SmtpPort, SecureSocketOptions.StartTls);
                smtpClient.Authenticate(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                smtpClient.Send(mimeMessage);
                smtpClient.Disconnect(quit: true);
            }
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private async void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
    {
        if (!Guid.TryParse(e.UserState?.ToString() ?? "", out var id) || (!e.Cancelled && e.Error == null))
        {
            return;
        }
        await Task.Delay(10000);
        using IServiceScope scope = _scopeFactory.CreateScope();
        IAppEmailService _appEmailService = scope.ServiceProvider.GetRequiredService<IAppEmailService>();
        AppEmail appEmail = await _appEmailService.GetById(id);
        if (appEmail != null)
        {
            await _appEmailService.Delete(appEmail);
        }
    }
}
