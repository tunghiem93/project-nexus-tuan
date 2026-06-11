namespace Nexus.User.Application;

public sealed class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPass { get; set; } = string.Empty;
    public string FromName { get; set; } = "Nexus";
    public string FromAddress { get; set; } = string.Empty;
    public string AppBaseUrl { get; set; } = string.Empty;
}
