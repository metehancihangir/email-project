using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/track")]
public class TrackingController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrackingController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("open/{campaignId}/{subscriberId}")]
    public async Task<IActionResult> TrackOpen(int campaignId, int subscriberId)
    {
        var recipient = await _context.CampaignRecipients
            .FirstOrDefaultAsync(r => r.CampaignId == campaignId && r.SubscriberId == subscriberId);

        if (recipient != null)
        {
            if (recipient.OpenedAt == null)
            {
                recipient.OpenedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // Return 1x1 transparent GIF
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return File(pixel, "image/gif");
    }

    [HttpGet("click/{linkToken}")]
    public async Task<IActionResult> TrackClick(string linkToken)
    {
        var trackedLink = await _context.TrackedLinks
            .FirstOrDefaultAsync(t => t.LinkToken == linkToken);

        if (trackedLink == null)
        {
            return NotFound();
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

        if (!Uri.TryCreate(trackedLink.OriginalUrl, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("Geçersiz yönlendirme bağlantısı.");
        }

        return Redirect(trackedLink.OriginalUrl);
    }
}
