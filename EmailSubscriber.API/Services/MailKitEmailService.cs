using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EmailSubscriber.API.Services;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IConfiguration configuration, ILogger<MailKitEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string? toName, string subject, string htmlBody)
    {
        var host = _configuration["Smtp:Host"];
        var portStr = _configuration["Smtp:Port"];
        var user = _configuration["Smtp:User"];
        var password = _configuration["Smtp:Password"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user))
        {
            _logger.LogWarning("SMTP ayarları eksik. E-posta konsola loglanıyor.");
            _logger.LogInformation("--- MOCK EMAIL START ---");
            _logger.LogInformation("To: {ToName} <{To}>", toName, to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {HtmlBody}", htmlBody);
            _logger.LogInformation("--- MOCK EMAIL END ---");
            return;
        }

        if (!int.TryParse(portStr, out int port))
        {
            port = 587; // default
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("SUBMAIL", user));
        message.ReplyTo.Add(new MailboxAddress("No Reply", "noreply@submail.com.tr"));
        message.To.Add(new MailboxAddress(toName ?? to, to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, password);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
