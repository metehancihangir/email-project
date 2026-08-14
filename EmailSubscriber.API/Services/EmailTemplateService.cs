using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net;

namespace EmailSubscriber.API.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IWebHostEnvironment _env;

    public EmailTemplateService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> GetConfirmationEmailHtmlAsync(string name, string confirmUrl)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Templates", "ConfirmationEmailTemplate.html");
        var html = await File.ReadAllTextAsync(filePath);
        
        var safeName = string.IsNullOrEmpty(name) ? "" : WebUtility.HtmlEncode(name);
        return html
            .Replace("{{Name}}", safeName)
            .Replace("{{ConfirmUrl}}", confirmUrl);
    }

    public async Task<string> GetWelcomeEmailHtmlAsync(string name, string unsubscribeUrl, string preferencesUrl)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Templates", "WelcomeEmailTemplate.html");
        var html = await File.ReadAllTextAsync(filePath);
        
        var safeName = string.IsNullOrEmpty(name) ? "" : " " + WebUtility.HtmlEncode(name);
        return html
            .Replace("{{Name}}", safeName)
            .Replace("{{UnsubscribeUrl}}", unsubscribeUrl)
            .Replace("{{PreferencesUrl}}", preferencesUrl);
    }
}
