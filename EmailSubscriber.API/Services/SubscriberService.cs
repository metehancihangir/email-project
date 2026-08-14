using EmailSubscriber.API.Data;
using EmailSubscriber.API.DTOs;
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

    public async Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name, string? interests = null)
    {
        var existingSubscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email);

        if (existingSubscriber != null)
        {
            if (existingSubscriber.IsActive && existingSubscriber.IsConfirmed)
            {
                return (false, "Conflict"); // 409
            }
            
            if (existingSubscriber.IsActive && !existingSubscriber.IsConfirmed)
            {
                if (existingSubscriber.ConfirmationTokenExpiresAt > DateTime.UtcNow)
                {
                    return (false, "Onay e-postası zaten gönderildi.");
                }
            }
            
            // İnaktif kullanıcı tekrar abone olmak istiyorsa VEYA aktif ama token süresi dolmuşsa
            existingSubscriber.IsActive = true;
            existingSubscriber.IsConfirmed = false;
            
            existingSubscriber.ConfirmationToken = Guid.NewGuid().ToString("N");
            existingSubscriber.ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            if (string.IsNullOrEmpty(existingSubscriber.UnsubscribeToken))
            {
                existingSubscriber.UnsubscribeToken = Guid.NewGuid().ToString("N");
            }
            existingSubscriber.Interests = interests;
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
                UnsubscribeToken = Guid.NewGuid().ToString("N"),
                Interests = interests
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
        string preferencesUrl = $"{_frontendUrl}/preferences?token={subscriber.UnsubscribeToken}";
        
        string htmlBody = await _templateService.GetWelcomeEmailHtmlAsync(subscriber.Name ?? "", unsubUrl, preferencesUrl);
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
        var result = await ResendConfirmationWithRateLimitAsync(email);
        if (result.IsSuccess)
            return (202, result.Message ?? "Onay e-postası gönderildi.");
        if (result.IsRateLimited)
            return (429, result.Message ?? "Lütfen bekleyin.");
        return (400, result.Message ?? "Geçersiz istek.");
    }

    public async Task<(int StatusCode, string Message, string? Email)> ValidateUnsubscribeTokenAsync(string token)
    {
        var subscriber = await _context.Subscribers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
        
        if (subscriber == null)
            return (404, "Kayıt veya bağlantı bulunamadı.", null);

        if (!subscriber.IsActive)
            return (200, "Abonelik zaten iptal edilmiş.", subscriber.Email);

        return (200, "Geçerli token.", subscriber.Email);
    }

    public async Task<(int StatusCode, string Message)> UnsubscribeAsync(string token)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
        
        if (subscriber == null)
            return (404, "Kayıt veya bağlantı bulunamadı.");

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        // Token'ı null yapıyoruz ki link tek kullanımlık olsun. Tekrar tıklandığında "Geçersiz Bağlantı" (404) versin.
        subscriber.UnsubscribeToken = null;
        
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();

        return (200, "Abonelik başarıyla iptal edildi.");
    }

    public async Task<(int StatusCode, string Message, string? Interests)> GetPreferencesAsync(string token)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
        if (subscriber == null || !subscriber.IsActive)
            return (404, "Kayıt bulunamadı.", null);

        return (200, "Başarılı.", subscriber.Interests);
    }

    public async Task<(int StatusCode, string Message)> UpdatePreferencesAsync(string token, string? interests)
    {
        var subscriber = await _context.Subscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == token);
        if (subscriber == null || !subscriber.IsActive)
            return (404, "Kayıt bulunamadı.");

        subscriber.Interests = interests;
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();

        return (200, "Tercihleriniz güncellendi.");
    }

    // ─── Faz 2: Rate-Limited Resend (Seçenek B — DB tabanlı) ────────────────

    /// <summary>
    /// 2.3.x: Cooldown kontrolü atomic UPDATE ile yapılır.
    /// Race condition koruması: tek SQL statement içinde hem kontrol hem güncelleme.
    /// </summary>
    public async Task<ResendCodeResult> ResendConfirmationWithRateLimitAsync(string email)
    {
        const int CooldownMinutes = 2;
        const int TokenTtlHours = 24;
        var now = DateTime.UtcNow;
        var cooldownThreshold = now.AddMinutes(-CooldownMinutes);

        int affected = 0;
        if (_context.Database.IsRelational())
        {
            affected = await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE Subscribers
                  SET LastCodeRequestedAt = {0}
                  WHERE Email = {1}
                    AND IsConfirmed = 0
                    AND (LastCodeRequestedAt IS NULL OR LastCodeRequestedAt < {2})",
                now, email, cooldownThreshold);
        }
        else
        {
            var memSub = await _context.Subscribers.FirstOrDefaultAsync(s => s.Email == email && !s.IsConfirmed && (s.LastCodeRequestedAt == null || s.LastCodeRequestedAt < cooldownThreshold));
            if (memSub != null)
            {
                memSub.LastCodeRequestedAt = now;
                await _context.SaveChangesAsync();
                affected = 1;
            }
        }

        if (affected == 0)
        {
            // Subscriber bulunamadı mı, onaylı mı, cooldown mu? Ayırt etmemek için:
            var sub = await _context.Subscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email == email);

            // 2.4.5: Email enumeration koruması — bulunamadı/onaylı için aynı generic mesaj
            if (sub == null || sub.IsConfirmed)
                return ResendCodeResult.GenericInvalid();

            // Cooldown aktif — kalan süreyi hesapla
            var nextAllowed = sub.LastCodeRequestedAt!.Value.AddMinutes(CooldownMinutes);
            var retryAfterSeconds = (int)Math.Ceiling((nextAllowed - now).TotalSeconds);
            return ResendCodeResult.RateLimited(Math.Max(retryAfterSeconds, 1), nextAllowed);
        }

        // 2.3.4: Cooldown geçti — yeni token üret ve DB'yi güncelle
        var subscriber = await _context.Subscribers.FirstAsync(s => s.Email == email);
        subscriber.ConfirmationToken = Guid.NewGuid().ToString("N");
        subscriber.ConfirmationTokenExpiresAt = now.AddHours(TokenTtlHours);
        await _context.SaveChangesAsync();

        // 2.3.5: Mevcut kuyruk üzerinden gönder
        string confirmUrl = $"{_frontendUrl}/confirm?token={subscriber.ConfirmationToken}";
        string htmlBody = await _templateService.GetConfirmationEmailHtmlAsync(subscriber.Name ?? "", confirmUrl);

        var job = new EmailSubscriber.API.Queue.EmailJob(
            To: subscriber.Email,
            ToName: subscriber.Name,
            Subject: "E-Bülten Abonelik Onayı",
            HtmlBody: htmlBody
        );
        _emailQueueService.Enqueue(job);
        _logger.LogInformation("Rate-limited resend: yeni onay e-postası kuyruğa eklendi: {Email}", email);

        // 2.3.6: nextAllowedAt = şimdiden 2 dakika sonra
        var nextAllowedAt = now.AddMinutes(CooldownMinutes);
        return ResendCodeResult.Success(nextAllowedAt);
    }

    /// <summary>
    /// 2.5.1-2.5.2: Sayfa yenileme koruması — tek source of truth (LastCodeRequestedAt).
    /// Frontend mount olduğunda bu endpoint'i çağırarak timer'ı senkronize eder.
    /// </summary>
    public async Task<(bool IsDisabled, DateTime? NextAllowedAt)> GetResendStatusAsync(string email)
    {
        const int CooldownMinutes = 2;
        var now = DateTime.UtcNow;

        var sub = await _context.Subscribers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == email && !s.IsConfirmed);

        if (sub?.LastCodeRequestedAt == null)
            return (false, null); // Hiç istek yapılmamış veya subscriber yok

        var nextAllowed = sub.LastCodeRequestedAt.Value.AddMinutes(CooldownMinutes);
        if (nextAllowed > now)
            return (true, nextAllowed); // Cooldown aktif

        return (false, null); // Cooldown süresi dolmuş
    }

    public async Task<PagedResult<PublicNewsletterDto>> GetPublicArchiveAsync(string? category, string? search, int page = 1, int pageSize = 12)
    {
        var query = _context.Campaigns
            .Include(c => c.Feedbacks)
            .AsNoTracking()
            .AsQueryable();

        // Test amaçlı oluşturulmuş bültenleri kamuya açık arşivden gizle
        query = query.Where(c => !c.Subject.StartsWith("Test") && 
                                 !c.Subject.StartsWith("[TEST]") && 
                                 !c.Subject.StartsWith("Geri Bildirim Testi"));

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("hepsi", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.Category == category || (c.Category == null && c.Subject.Contains(category)));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Subject.Contains(search) || c.HtmlBody.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var rawList = await query
            .OrderByDescending(c => c.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Subject,
                c.HtmlBody,
                c.Category,
                c.CoverImageUrl,
                c.SentAt,
                Likes = c.Feedbacks.Count(f => f.IsPositive)
            })
            .ToListAsync();

        var items = rawList.Select(c =>
        {
            // HTML etiketlerini temizleyip kısa özet oluştur
            var cleanText = System.Text.RegularExpressions.Regex.Replace(c.HtmlBody, "<.*?>", " ");
            cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"\s+", " ").Trim();
            var excerpt = cleanText.Length > 180 ? cleanText[..180] + "..." : cleanText;

            return new PublicNewsletterDto
            {
                Id = c.Id,
                Subject = c.Subject,
                Excerpt = excerpt,
                Category = c.Category ?? "Genel",
                CoverImageUrl = c.CoverImageUrl,
                SentAt = c.SentAt,
                LikesCount = c.Likes
            };
        }).ToList();

        return new PagedResult<PublicNewsletterDto>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PublicNewsletterDto?> GetPublicNewsletterByIdAsync(int id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Feedbacks)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        var cleanText = System.Text.RegularExpressions.Regex.Replace(campaign.HtmlBody, "<.*?>", " ");
        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"\s+", " ").Trim();
        var excerpt = cleanText.Length > 180 ? cleanText[..180] + "..." : cleanText;

        return new PublicNewsletterDto
        {
            Id = campaign.Id,
            Subject = campaign.Subject,
            Excerpt = excerpt,
            HtmlBody = campaign.HtmlBody,
            Category = campaign.Category ?? "Genel",
            CoverImageUrl = campaign.CoverImageUrl,
            SentAt = campaign.SentAt,
            LikesCount = campaign.Feedbacks.Count(f => f.IsPositive)
        };
    }
}
