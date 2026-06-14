namespace Nexus.User.Application;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
