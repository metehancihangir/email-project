using EmailSubscriber.API.Data;
using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmailSubscriber.API.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly EmailSubscriber.API.Queue.IEmailQueueService _emailQueue;
    private readonly ITrackingService _trackingService;

    public AdminService(
        AppDbContext context, 
        IConfiguration configuration, 
        EmailSubscriber.API.Queue.IEmailQueueService emailQueue,
        ITrackingService trackingService)
    {
        _context = context;
        _configuration = configuration;
        _emailQueue = emailQueue;
        _trackingService = trackingService;
    }

    public Task<string?> LoginAsync(string username, string password)
    {
        var adminUsername = _configuration["Admin:Username"];
        var adminHash = _configuration["Admin:PasswordHash"];

        if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminHash))
        {
            return Task.FromResult<string?>(null);
        }

        if (!string.Equals(username?.Trim(), adminUsername.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<string?>(null);
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password ?? string.Empty, adminHash);

        if (!isPasswordValid)
        {
            return Task.FromResult<string?>(null);
        }

        // Generate JWT Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult<string?>(tokenHandler.WriteToken(token));
    }

    public async Task<PagedResult<SubscriberDto>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed, int page = 1, int pageSize = 20)
    {
        var query = _context.Subscribers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.Email.Contains(search) || (s.Name != null && s.Name.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (isConfirmed.HasValue)
        {
            query = query.Where(s => s.IsConfirmed == isConfirmed.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query.OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubscriberDto
            {
                Id = s.Id,
                Email = s.Email,
                Name = s.Name,
                IsConfirmed = s.IsConfirmed,
                IsActive = s.IsActive,
                SubscribedAt = s.SubscribedAt,
                UnsubscribedAt = s.UnsubscribedAt
            })
            .ToListAsync();

        return new PagedResult<SubscriberDto>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> DeactivateSubscriberAsync(int id)
    {
        var subscriber = await _context.Subscribers.FindAsync(id);
        if (subscriber == null) return false;

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        _context.Subscribers.Update(subscriber);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSubscriberAsync(int id)
    {
        var subscriber = await _context.Subscribers.FindAsync(id);
        if (subscriber == null) return false;

        _context.Subscribers.Remove(subscriber);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetStatsAsync()
    {
        var total = await _context.Subscribers.CountAsync();
        var active = await _context.Subscribers.CountAsync(s => s.IsActive);
        var unconfirmed = await _context.Subscribers.CountAsync(s => !s.IsConfirmed);
        
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);
        var today = await _context.Subscribers.CountAsync(s => s.SubscribedAt >= todayStart && s.SubscribedAt < todayEnd);

        return new { total, active, unconfirmed, today };
    }

    public async Task<object> GetGrowthChartAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-30);

        var growthDataRaw = await _context.Subscribers
            .Where(s => s.SubscribedAt >= thirtyDaysAgo)
            .GroupBy(s => s.SubscribedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var growthData = growthDataRaw
            .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), count = x.Count })
            .ToList();

        return growthData;
    }

    public Task<(int campaignId, int recipientCount)> SendNewsletterAsync(string subject, string htmlBody, string? category = null, string? coverImageUrl = null)
    {
        return SendInternalAsync(subject, htmlBody, null, category, coverImageUrl);
    }

    public Task<(int campaignId, int recipientCount)> SendTargetedNewsletterAsync(string subject, string htmlBody, List<int> subscriberIds, string? category = null, string? coverImageUrl = null)
    {
        return SendInternalAsync(subject, htmlBody, subscriberIds, category, coverImageUrl);
    }

    public async Task<bool> SendTestEmailAsync(string targetEmail, string subject, string htmlBody, string? category = null, string? coverImageUrl = null)
    {
        var sanitizer = new Ganss.Xss.HtmlSanitizer();
        var safeHtmlBody = sanitizer.Sanitize(htmlBody);

        string coverHeaderHtml = !string.IsNullOrEmpty(coverImageUrl) && !safeHtmlBody.Contains(coverImageUrl)
            ? $"<div style='text-align: center; margin-bottom: 24px;'><img src='{coverImageUrl}' alt='Bülten Görseli' style='max-width: 100%; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08);' /></div>"
            : "";

        string finalBody = $@"
        <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 620px; margin: 0 auto; color: #1e293b; line-height: 1.6; padding: 20px 10px;'>
            <div style='background: #f8fafc; border: 1px dashed #cbd5e1; padding: 10px; border-radius: 8px; font-size: 12px; color: #64748b; text-align: center; margin-bottom: 20px;'>
                🧪 <b>TEST E-POSTASI</b> — Bu bir yönetici önizleme iletisidir.
            </div>
            <p style='font-size: 16px; margin-bottom: 18px;'>Merhaba <b>Yönetici</b>,</p>
            {coverHeaderHtml}
            <div style='background: white; padding: 24px; border-radius: 16px; border: 1px solid #e2e8f0; box-shadow: 0 1px 3px rgba(0,0,0,0.05);'>
                {safeHtmlBody}
            </div>
            <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 30px 0;' />
            <p style='font-size: 12px; color: #94a3b8; text-align: center;'>SUBMAIL Test Gönderimi</p>
        </div>";

        var job = new EmailSubscriber.API.Queue.EmailJob(targetEmail, "Yönetici (Test)", $"[TEST] {subject}", finalBody);
        _emailQueue.Enqueue(job);
        return true;
    }

    private async Task<(int campaignId, int recipientCount)> SendInternalAsync(string subject, string htmlBody, List<int>? targetSubscriberIds, string? category = null, string? coverImageUrl = null)
    {
        var query = _context.Subscribers.Where(s => s.IsActive && s.IsConfirmed);
        
        if (targetSubscriberIds != null)
        {
            query = query.Where(s => targetSubscriberIds.Contains(s.Id));
        }

        var activeSubscribers = await query.ToListAsync();

        // Eğer belirli bir kategoriye özel bülten gönderiliyorsa ve özel hedef liste verilmemişse,
        // sadece bu kategoriyi ilgi alanlarına (Interests) eklemiş abonelere gönder.
        if (targetSubscriberIds == null && !string.IsNullOrWhiteSpace(category) && !category.Equals("Genel", StringComparison.OrdinalIgnoreCase))
        {
            activeSubscribers = activeSubscribers
                .Where(s => !string.IsNullOrWhiteSpace(s.Interests) &&
                            s.Interests.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Contains(category, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        int recipientCount = activeSubscribers.Count;

        // XSS Koruması
        var sanitizer = new Ganss.Xss.HtmlSanitizer();
        var safeHtmlBody = sanitizer.Sanitize(htmlBody);

        var campaign = new Campaign
        {
            Subject = subject,
            HtmlBody = safeHtmlBody,
            Category = category,
            CoverImageUrl = coverImageUrl,
            SentAt = DateTime.UtcNow,
            RecipientCount = recipientCount
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var host = _configuration["AppUrl"] ?? "http://localhost:5117";
        var frontendUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5173";

        string coverHeaderHtml = !string.IsNullOrEmpty(coverImageUrl) && !safeHtmlBody.Contains(coverImageUrl)
            ? $"<div style='text-align: center; margin-bottom: 24px;'><img src='{coverImageUrl}' alt='{category ?? "Bülten"}' style='max-width: 100%; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.08);' /></div>"
            : "";
        
        foreach (var sub in activeSubscribers)
        {
            var recipient = new CampaignRecipient
            {
                CampaignId = campaign.Id,
                SubscriberId = sub.Id,
                SentAt = DateTime.UtcNow,
                Status = "pending"
            };

            _context.CampaignRecipients.Add(recipient);
            await _context.SaveChangesAsync();

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(safeHtmlBody);
            
            var aNodes = doc.DocumentNode.SelectNodes("//a[@href]");
            if (aNodes != null)
            {
                foreach (var a in aNodes)
                {
                    string originalHref = a.GetAttributeValue("href", "");
                    if (!string.IsNullOrWhiteSpace(originalHref) && !originalHref.StartsWith("mailto:") && !originalHref.StartsWith("tel:"))
                    {
                        var linkToken = Guid.NewGuid().ToString("N");
                        var trackedLink = new TrackedLink
                        {
                            CampaignId = campaign.Id,
                            SubscriberId = sub.Id,
                            OriginalUrl = originalHref,
                            LinkToken = linkToken
                        };
                        _context.TrackedLinks.Add(trackedLink);
                        
                        a.SetAttributeValue("href", $"{host}/api/track/click/{linkToken}");
                    }
                }
                await _context.SaveChangesAsync();
            }

            string sig = _trackingService.GenerateOpenSignature(campaign.Id, sub.Id);
            string trackingPixel = $"<img src=\"{host}/api/track/open/{campaign.Id}/{sub.Id}?sig={sig}\" width=\"1\" height=\"1\" style=\"display:none;\" />";
            
            var unsubToken = sub.UnsubscribeToken;
            string unsubscribeLink = $"{frontendUrl}/unsubscribe?token={unsubToken}";
            string preferencesLink = $"{frontendUrl}/preferences?token={unsubToken}";
            string webViewLink = $"{frontendUrl}/arsiv/{campaign.Id}";
            string positiveFeedbackLink = $"{host}/api/track/feedback/{campaign.Id}/{sub.Id}?isPositive=true&sig={sig}";
            string negativeFeedbackLink = $"{host}/api/track/feedback/{campaign.Id}/{sub.Id}?isPositive=false&sig={sig}";

            string greetingName = !string.IsNullOrWhiteSpace(sub.Name) ? sub.Name.Trim() : "Değerli Okurumuz";

            string finalBody = $@"
            <div style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; max-width: 620px; margin: 0 auto; color: #1e293b; line-height: 1.6; padding: 20px 10px;'>
                
                <!-- Web View Link -->
                <div style='text-align: right; margin-bottom: 16px;'>
                    <a href='{webViewLink}' style='font-size: 11px; color: #94a3b8; text-decoration: underline;'>Bu e-postayı tarayıcınızda görüntüleyin</a>
                </div>

                <!-- Kişiselleştirilmiş Selamlama -->
                <p style='font-size: 16px; margin-bottom: 18px; color: #334155;'>Merhaba <b>{greetingName}</b>,</p>

                <!-- Kapak Görseli -->
                {coverHeaderHtml}

                <!-- Bülten Ana İçeriği -->
                <div style='background: white; padding: 24px; border-radius: 16px; border: 1px solid #e2e8f0; box-shadow: 0 1px 3px rgba(0,0,0,0.05);'>
                    {doc.DocumentNode.OuterHtml}
                </div>

                <!-- Geri Bildirim Bölümü (Feedback Widget) -->
                <div style='margin-top: 32px; padding: 20px; background: #f8fafc; border-radius: 14px; text-align: center; border: 1px solid #e2e8f0;'>
                    <p style='font-size: 14px; font-weight: 600; color: #475569; margin: 0 0 12px;'>Bu bülteni nasıl buldunuz?</p>
                    <div style='display: inline-flex; gap: 12px;'>
                        <a href='{positiveFeedbackLink}' style='display: inline-block; background: #ffffff; color: #16a34a; border: 1px solid #bbf7d0; padding: 8px 18px; border-radius: 10px; font-size: 13px; font-weight: 600; text-decoration: none; box-shadow: 0 1px 2px rgba(0,0,0,0.05);'>👍 Faydalı Buldum</a>
                        <a href='{negativeFeedbackLink}' style='display: inline-block; background: #ffffff; color: #dc2626; border: 1px solid #fecaca; padding: 8px 18px; border-radius: 10px; font-size: 13px; font-weight: 600; text-decoration: none; box-shadow: 0 1px 2px rgba(0,0,0,0.05);'>👎 Geliştirilmeli</a>
                    </div>
                </div>

                {trackingPixel}

                <!-- Altbilgi (Footer) -->
                <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 30px 0 20px;' />
                <div style='text-align: center; color: #94a3b8; font-size: 12px; line-height: 1.5;'>
                    <p style='margin: 0 0 8px;'>Bu bülten SUBMAIL tarafından ilgi alanlarınıza özel olarak oluşturulmuştur.</p>
                    <p style='margin: 0;'>
                        <a href='{preferencesLink}' style='color: #64748b; text-decoration: underline;'>Tercihleri Güncelle</a>
                        <span style='margin: 0 6px;'>•</span>
                        <a href='{unsubscribeLink}' style='color: #64748b; text-decoration: underline;'>Abonelikten Ayrıl</a>
                    </p>
                </div>
            </div>";

            var job = new EmailSubscriber.API.Queue.EmailJob(sub.Email, sub.Name, subject, finalBody, recipient.Id);
            _emailQueue.Enqueue(job);
        }

        return (campaign.Id, recipientCount);
    }

    public async Task<IEnumerable<CampaignDto>> GetCampaignsAsync()
    {
        return await _context.Campaigns
            .Include(c => c.Recipients)
            .Include(c => c.Feedbacks)
            .OrderByDescending(c => c.SentAt)
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Subject = c.Subject,
                SentAt = c.SentAt,
                RecipientCount = c.RecipientCount,
                Category = c.Category,
                CoverImageUrl = c.CoverImageUrl,
                OpenedCount = c.Recipients.Count(r => r.OpenedAt != null),
                OpenRate = c.RecipientCount > 0 ? Math.Round((double)c.Recipients.Count(r => r.OpenedAt != null) / c.RecipientCount * 100, 2) : 0,
                ClickedCount = c.Recipients.Count(r => r.ClickedAt != null),
                ClickRate = c.RecipientCount > 0 ? Math.Round((double)c.Recipients.Count(r => r.ClickedAt != null) / c.RecipientCount * 100, 2) : 0,
                PositiveFeedbackCount = c.Feedbacks.Count(f => f.IsPositive),
                TotalFeedbackCount = c.Feedbacks.Count,
                PositiveFeedbackRate = c.Feedbacks.Any() ? Math.Round((double)c.Feedbacks.Count(f => f.IsPositive) / c.Feedbacks.Count * 100, 1) : 0
            })
            .ToListAsync();
    }

    public async Task<CampaignStatsDto?> GetCampaignStatsAsync(int id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Recipients)
            .Include(c => c.Feedbacks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        int sent = campaign.Recipients.Count(r => r.Status == "sent");
        int opened = campaign.Recipients.Count(r => r.OpenedAt != null);
        int clicked = campaign.Recipients.Count(r => r.ClickedAt != null);
        int positiveFeedback = campaign.Feedbacks.Count(f => f.IsPositive);
        int totalFeedback = campaign.Feedbacks.Count;

        return new CampaignStatsDto
        {
            Sent = sent,
            Opened = opened,
            OpenRate = sent > 0 ? Math.Round((double)opened / sent * 100, 2) : 0,
            Clicked = clicked,
            ClickRate = sent > 0 ? Math.Round((double)clicked / sent * 100, 2) : 0,
            PositiveFeedbackCount = positiveFeedback,
            TotalFeedbackCount = totalFeedback,
            PositiveFeedbackRate = totalFeedback > 0 ? Math.Round((double)positiveFeedback / totalFeedback * 100, 1) : 0
        };
    }

    public async Task<string?> GetCampaignContentAsync(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        return campaign?.HtmlBody;
    }
}
