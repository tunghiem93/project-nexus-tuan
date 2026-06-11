using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Nexus.User.Application;

namespace Nexus.User.Api.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.SmtpUser) || string.IsNullOrWhiteSpace(_options.SmtpPass) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("SMTP email settings are not configured.");
        }

        using var message = new MailMessage();
        message.From = new MailAddress(_options.FromAddress, _options.FromName);
        message.To.Add(email);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPass)
        };

        return client.SendMailAsync(message, cancellationToken);
    }
}
