using EmailSubscriber.API.Data;
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

    public AdminService(AppDbContext context, IConfiguration configuration, EmailSubscriber.API.Queue.IEmailQueueService emailQueue)
    {
        _context = context;
        _configuration = configuration;
        _emailQueue = emailQueue;
    }

    public Task<string?> LoginAsync(string username, string password)
    {
        var adminUsername = _configuration["Admin:Username"];
        var adminHash = _configuration["Admin:PasswordHash"];

        if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminHash))
        {
            return Task.FromResult<string?>(null);
        }

        if (username != adminUsername)
        {
            return Task.FromResult<string?>(null);
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, adminHash);

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

    public async Task<IEnumerable<Subscriber>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed)
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

        // Güvenlik için tokenları boşaltıyoruz
        var list = await query.OrderByDescending(s => s.SubscribedAt).ToListAsync();
        
        list.ForEach(s => 
        {
            s.ConfirmationToken = null;
            s.ConfirmationTokenExpiresAt = null;
        });

        return list;
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

    public async Task<(int campaignId, int recipientCount)> SendNewsletterAsync(string subject, string htmlBody)
    {
        var activeSubscribers = await _context.Subscribers
            .Where(s => s.IsActive && s.IsConfirmed)
            .ToListAsync();

        int recipientCount = activeSubscribers.Count;

        var campaign = new Campaign
        {
            Subject = subject,
            HtmlBody = htmlBody,
            SentAt = DateTime.UtcNow,
            RecipientCount = recipientCount
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var host = _configuration["AppUrl"] ?? "http://localhost:5117";
        
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
            doc.LoadHtml(htmlBody);
            
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

            string trackingPixel = $"<img src=\"{host}/api/track/open/{campaign.Id}/{sub.Id}\" width=\"1\" height=\"1\" style=\"display:none;\" />";
            string bodyWithTracking = doc.DocumentNode.OuterHtml + trackingPixel;

            var unsubToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sub.Email));
            string frontendUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5173";
            string unsubscribeLink = $"{frontendUrl}/unsubscribe?token={unsubToken}";
            string finalBody = bodyWithTracking + $"<br><br><small><a href='{unsubscribeLink}'>Abonelikten Ayrıl</a></small>";

            var job = new EmailSubscriber.API.Queue.EmailJob(sub.Email, sub.Name, subject, finalBody, recipient.Id);
            _emailQueue.Enqueue(job);
        }

        return (campaign.Id, recipientCount);
    }

    public async Task<IEnumerable<EmailSubscriber.API.DTOs.CampaignDto>> GetCampaignsAsync()
    {
        return await _context.Campaigns
            .Include(c => c.Recipients)
            .OrderByDescending(c => c.SentAt)
            .Select(c => new EmailSubscriber.API.DTOs.CampaignDto
            {
                Id = c.Id,
                Subject = c.Subject,
                SentAt = c.SentAt,
                RecipientCount = c.RecipientCount,
                OpenedCount = c.Recipients.Count(r => r.OpenedAt != null),
                OpenRate = c.RecipientCount > 0 ? Math.Round((double)c.Recipients.Count(r => r.OpenedAt != null) / c.RecipientCount * 100, 2) : 0,
                ClickedCount = c.Recipients.Count(r => r.ClickedAt != null),
                ClickRate = c.RecipientCount > 0 ? Math.Round((double)c.Recipients.Count(r => r.ClickedAt != null) / c.RecipientCount * 100, 2) : 0
            })
            .ToListAsync();
    }

    public async Task<EmailSubscriber.API.DTOs.CampaignStatsDto?> GetCampaignStatsAsync(int id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return null;

        int sent = campaign.Recipients.Count(r => r.Status == "sent");
        int opened = campaign.Recipients.Count(r => r.OpenedAt != null);
        int clicked = campaign.Recipients.Count(r => r.ClickedAt != null);

        return new EmailSubscriber.API.DTOs.CampaignStatsDto
        {
            Sent = sent,
            Opened = opened,
            OpenRate = sent > 0 ? Math.Round((double)opened / sent * 100, 2) : 0,
            Clicked = clicked,
            ClickRate = sent > 0 ? Math.Round((double)clicked / sent * 100, 2) : 0
        };
    }
}
