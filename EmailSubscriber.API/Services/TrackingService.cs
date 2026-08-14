using EmailSubscriber.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EmailSubscriber.API.Services;

public class TrackingService : ITrackingService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public TrackingService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public string GenerateOpenSignature(int campaignId, int subscriberId)
    {
        var secret = _configuration["Jwt:Secret"] ?? "SubmailDefaultOpenTrackingSecret!";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{campaignId}:{subscriberId}"));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    public bool ValidateOpenSignature(int campaignId, int subscriberId, string? signature)
    {
        if (string.IsNullOrWhiteSpace(signature)) return false;
        var expected = GenerateOpenSignature(campaignId, subscriberId);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant())
        );
    }

    public async Task<bool> TrackOpenAsync(int campaignId, int subscriberId, string? signature)
    {
        if (!ValidateOpenSignature(campaignId, subscriberId, signature))
        {
            return false;
        }

        var recipient = await _context.CampaignRecipients
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId);

        if (recipient != null && recipient.OpenedAt == null)
        {
            recipient.OpenedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<(bool IsSuccess, string? OriginalUrl)> TrackClickAsync(string linkToken)
    {
        var trackedLink = await _context.TrackedLinks
            .FirstOrDefaultAsync(t => t.LinkToken == linkToken);

        if (trackedLink == null)
        {
            return (false, null);
        }

        if (trackedLink.ClickedAt == null)
        {
            trackedLink.ClickedAt = DateTime.UtcNow;
            
            var recipient = await _context.CampaignRecipients
                .FirstOrDefaultAsync(r => r.CampaignId == trackedLink.CampaignId && r.SubscriberId == trackedLink.SubscriberId);

            if (recipient != null && recipient.ClickedAt == null)
            {
                recipient.ClickedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        return (true, trackedLink.OriginalUrl);
    }
}
