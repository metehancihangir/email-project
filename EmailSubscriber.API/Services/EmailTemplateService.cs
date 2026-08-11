using Microsoft.AspNetCore.Hosting;
using System.IO;

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
        
        return html
            .Replace("{{Name}}", string.IsNullOrEmpty(name) ? "" : name)
            .Replace("{{ConfirmUrl}}", confirmUrl);
    }

    public async Task<string> GetWelcomeEmailHtmlAsync(string name, string unsubscribeUrl)
    {
        var filePath = Path.Combine(_env.ContentRootPath, "Templates", "WelcomeEmailTemplate.html");
        var html = await File.ReadAllTextAsync(filePath);
        
        return html
            .Replace("{{Name}}", string.IsNullOrEmpty(name) ? "" : " " + name)
            .Replace("{{UnsubscribeUrl}}", unsubscribeUrl);
    }
}
