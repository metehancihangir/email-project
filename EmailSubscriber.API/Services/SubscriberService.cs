using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services;

public class SubscriberService : ISubscriberService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriberService> _logger;
    private readonly EmailSubscriber.API.Queue.IEmailQueueService _emailQueueService;
    private readonly IEmailTemplateService _templateService;
    private readonly string _frontendUrl;

    public SubscriberService(
        AppDbContext context, 
        ILogger<SubscriberService> logger, 
        EmailSubscriber.API.Queue.IEmailQueueService emailQueueService,
        IEmailTemplateService templateService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _emailQueueService = emailQueueService;
        _templateService = templateService;
        _frontendUrl = configuration["App:BaseUrl"] ?? "http://localhost:5173";
    }

    public async Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name)
    {
        var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);

        if (existingSubscriber != null)
        {
            if (existingSubscriber.IsConfirmed)
            {
                return (false, "Conflict"); // 409
            }
            
            if (existingSubscriber.ConfirmationTokenExpiresAt > DateTime.UtcNow)
            {
                return (false, "Onay e-postası zaten gönderildi.");
            }
            
            existingSubscriber.ConfirmationToken = Guid.NewGuid().ToString("N");
            existingSubscriber.ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            existingSubscriber.LastVerificationCodeSentAt = DateTime.UtcNow;
            if (string.IsNullOrEmpty(existingSubscriber.UnsubscribeToken))
            {
                existingSubscriber.UnsubscribeToken = Guid.NewGuid().ToString("N");
            }
            _context.Subscribers.Update(existingSubscriber);
        }
        else
        {
            var newSubscriber = new Subscriber
            {
                Email = email,
                Name = name,
                IsConfirmed = false,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow,
                ConfirmationToken = Guid.NewGuid().ToString("N"),
                ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
                LastVerificationCodeSentAt = DateTime.UtcNow,
                UnsubscribeToken = Guid.NewGuid().ToString("N")
            };
            
            _context.Subscribers.Add(newSubscriber);
            existingSubscriber = newSubscriber;
        }

        await _context.SaveChangesAsync();

        string confirmUrl = $"{_frontendUrl}/confirm?token={existingSubscriber.ConfirmationToken}";
        string htmlBody = await _templateService.GetConfirmationEmailHtmlAsync(existingSubscriber.Name ?? "", confirmUrl);
        
        var job = new EmailSubscriber.API.Queue.EmailJob(
            To: existingSubscriber.Email,
            ToName: existingSubscriber.Name,
            Subject: "E-Bülten Abonelik Onayı",
            HtmlBody: htmlBody
        );

        _emailQueueService.Enqueue(job);
        _logger.LogInformation("Onay e-postası görevi kuyruğa eklendi: {Email}", existingSubscriber.Email);

        return (true, "Onay e-postası gönderildi!");
    }

    public async Task<(int StatusCode, string Message)> ConfirmSubscriberAsync(string token)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.ConfirmationToken == token);

        if (subscriber == null)
            return (404, "Kayıt bulunamadı.");

        if (subscriber.IsConfirmed)
            return (200, "Zaten onaylı.");

        if (subscriber.ConfirmationTokenExpiresAt < DateTime.UtcNow)
            return (410, "Token süresi dolmuş.");

        // Onayla
        subscriber.IsConfirmed = true;
        // Idempotent (tekrar 200 OK dönebilmesi) olabilmesi için token'ı null'a çekiyoruz ancak
        // IsConfirmed zaten true olduğu için ilerideki isteklerde "Zaten onaylı" dönecek.
        subscriber.ConfirmationToken = null;
        subscriber.ConfirmationTokenExpiresAt = null;

        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();

        // Hoş geldin e-postası gönder
        string unsubUrl = $"{_frontendUrl}/unsubscribe?token={subscriber.UnsubscribeToken}";
        
        string htmlBody = await _templateService.GetWelcomeEmailHtmlAsync(subscriber.Name ?? "", unsubUrl);
        var job = new EmailSubscriber.API.Queue.EmailJob(
            To: subscriber.Email,
            ToName: subscriber.Name,
            Subject: "Aramıza Hoş Geldiniz!",
            HtmlBody: htmlBody
        );

        _emailQueueService.Enqueue(job);
        return (200, "Onaylandı.");
    }

    public async Task<(int StatusCode, string Message)> ResendConfirmationAsync(string email)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
        if (subscriber == null || subscriber.IsConfirmed)
            return (400, "Geçersiz istek.");

        if (subscriber.LastVerificationCodeSentAt.HasValue && 
            subscriber.LastVerificationCodeSentAt.Value.AddMinutes(2) > DateTime.UtcNow)
        {
            return (429, "Yeni kod talep etmek için lütfen 2 dakika bekleyin.");
        }

        subscriber.ConfirmationToken = Guid.NewGuid().ToString("N");
        subscriber.ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        subscriber.LastVerificationCodeSentAt = DateTime.UtcNow;
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();

        string confirmUrl = $"{_frontendUrl}/confirm?token={subscriber.ConfirmationToken}";
        string htmlBody = await _templateService.GetConfirmationEmailHtmlAsync(subscriber.Name ?? "", confirmUrl);
        
        var job = new EmailSubscriber.API.Queue.EmailJob(
            To: subscriber.Email,
            ToName: subscriber.Name,
            Subject: "E-Bülten Abonelik Onayı",
            HtmlBody: htmlBody
        );

        _emailQueueService.Enqueue(job);
        return (202, "E-posta gönderildi.");
    }

    public async Task<(int StatusCode, string Message)> UnsubscribeAsync(string token)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
        
        if (subscriber == null)
            return (404, "Kayıt bulunamadı.");

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();

        return (200, "Abonelik iptal edildi.");
    }
}
