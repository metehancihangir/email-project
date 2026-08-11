namespace EmailSubscriber.API.Services;

public interface IEmailService
{
    Task SendAsync(string to, string? toName, string subject, string htmlBody);
}
