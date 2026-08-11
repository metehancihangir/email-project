using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services;

public class SubscriberService : ISubscriberService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriberService> _logger;
    private readonly EmailSubscriber.API.Queue.IEmailQueueService _emailQueueService;

    public SubscriberService(
        AppDbContext context, 
        ILogger<SubscriberService> logger, 
        EmailSubscriber.API.Queue.IEmailQueueService emailQueueService)
    {
        _context = context;
        _logger = logger;
        _emailQueueService = emailQueueService;
    }

    public async Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name)
    {
        var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);

        if (existingSubscriber != null)
        {
            if (existingSubscriber.IsConfirmed)
            {
                return (false, "Conflict"); // 409 dönülecek.
            }
            
            // Eğer varsa ve henüz onaylanmamışsa, süresi geçmemişse beklesin.
            if (existingSubscriber.ConfirmationTokenExpiresAt > DateTime.UtcNow)
            {
                return (false, "Onay e-postası zaten gönderildi.");
            }
            
            // Onaylanmamış ama süresi geçmiş, tokenı yenileyelim.
            existingSubscriber.ConfirmationToken = Guid.NewGuid().ToString("N");
            existingSubscriber.ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
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
                ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };
            
            _context.Subscribers.Add(newSubscriber);
            existingSubscriber = newSubscriber;
        }

        await _context.SaveChangesAsync();

        // E-posta gönderimi kuyruğa alınır (Arka planda işlenecek)
        string confirmUrl = $"http://localhost:5173/confirm?token={existingSubscriber.ConfirmationToken}";
        string htmlBody = $"<h2>Merhaba {existingSubscriber.Name ?? existingSubscriber.Email},</h2><p>Aboneliğinizi onaylamak için lütfen <a href='{confirmUrl}'>buraya tıklayın</a>.</p>";
        
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
}
