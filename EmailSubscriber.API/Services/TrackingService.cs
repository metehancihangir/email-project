using EmailSubscriber.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Services;

public class TrackingService : ITrackingService
{
    private readonly AppDbContext _context;

    public TrackingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TrackOpenAsync(int campaignId, int subscriberId)
    {
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
