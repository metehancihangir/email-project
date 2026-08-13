namespace EmailSubscriber.API.Services;

public interface IEmailTemplateService
{
    Task<string> GetConfirmationEmailHtmlAsync(string name, string confirmUrl);
    Task<string> GetWelcomeEmailHtmlAsync(string name, string unsubscribeUrl, string preferencesUrl);
}
