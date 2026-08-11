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

        var growthData = await _context.Subscribers
            .Where(s => s.SubscribedAt >= thirtyDaysAgo)
            .GroupBy(s => s.SubscribedAt.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date)
            .ToListAsync();

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

            string unsubscribeLink = $"{host}/api/subscribers/unsubscribe?token={sub.ConfirmationToken}";
            string bodyWithUnsubscribe = htmlBody + $"<br><br><small><a href='{unsubscribeLink}'>Abonelikten Ayrıl</a></small>";

            var job = new EmailSubscriber.API.Queue.EmailJob(sub.Email, sub.Name, subject, bodyWithUnsubscribe, recipient.Id);
            _emailQueue.Enqueue(job);
        }

        return (campaign.Id, recipientCount);
    }

    public async Task<IEnumerable<EmailSubscriber.API.DTOs.CampaignDto>> GetCampaignsAsync()
    {
        return await _context.Campaigns
            .OrderByDescending(c => c.SentAt)
            .Select(c => new EmailSubscriber.API.DTOs.CampaignDto
            {
                Id = c.Id,
                Subject = c.Subject,
                SentAt = c.SentAt,
                RecipientCount = c.RecipientCount
            })
            .ToListAsync();
    }
}
