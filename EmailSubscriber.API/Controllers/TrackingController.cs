using EmailSubscriber.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/track")]
[EnableRateLimiting("Api")]
public class TrackingController : ControllerBase
{
    private readonly EmailSubscriber.API.Services.ITrackingService _trackingService;
    private readonly IConfiguration _configuration;

    public TrackingController(EmailSubscriber.API.Services.ITrackingService trackingService, IConfiguration configuration)
    {
        _trackingService = trackingService;
        _configuration = configuration;
    }

    [HttpGet("open/{campaignId}/{subscriberId}")]
    public async Task<IActionResult> TrackOpen(int campaignId, int subscriberId, [FromQuery] string? sig)
    {
        await _trackingService.TrackOpenAsync(campaignId, subscriberId, sig);

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

    [HttpGet("feedback/{campaignId}/{subscriberId}")]
    public async Task<IActionResult> TrackFeedback(int campaignId, int subscriberId, [FromQuery] bool isPositive, [FromQuery] string? sig)
    {
        var success = await _trackingService.TrackFeedbackAsync(campaignId, subscriberId, isPositive, sig);

        var frontendUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5173";
        var feedbackParam = isPositive ? "ok" : "negative";

        if (!success)
        {
            // Geçersiz imzada bile kullanıcıyı arşive yönlendir; hata bilgisini URL'de taşımak gerekmez.
            return Redirect($"{frontendUrl}/arsiv/{campaignId}");
        }

        // Geri bildirim kaydedildi — kullanıcıyı bültene yönlendir, frontend toast gösterir.
        return Redirect($"{frontendUrl}/arsiv/{campaignId}?feedback={feedbackParam}");
    }
}
