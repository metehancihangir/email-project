using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services;

public class SubscriberService : ISubscriberService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SubscriberService> _logger;

    public SubscriberService(AppDbContext context, ILogger<SubscriberService> logger)
    {
        _context = context;
        _logger = logger;
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

        // Şimdilik ILogger ile simüle ediyoruz (Faz 2'de e-posta kuyruğa gidecek)
        _logger.LogInformation("SIMULATION: Onay e-postası gönderildi -> Kime: {Email}, Token: {Token}", email, existingSubscriber.ConfirmationToken);

        return (true, "Onay e-postası gönderildi!");
    }
}
