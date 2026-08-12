using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/track")]
public class TrackingController : ControllerBase
{
    private readonly EmailSubscriber.API.Services.ITrackingService _trackingService;

    public TrackingController(EmailSubscriber.API.Services.ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    [HttpGet("open/{campaignId}/{subscriberId}")]
    public async Task<IActionResult> TrackOpen(int campaignId, int subscriberId)
    {
        await _trackingService.TrackOpenAsync(campaignId, subscriberId);

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
        var result = await _trackingService.TrackClickAsync(linkToken);

        if (!result.IsSuccess || result.OriginalUrl == null)
        {
            return NotFound();
        }

        if (!Uri.TryCreate(result.OriginalUrl, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("Geçersiz yönlendirme bağlantısı.");
        }

        return Redirect(result.OriginalUrl);
    }
}
