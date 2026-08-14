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

    public TrackingController(EmailSubscriber.API.Services.ITrackingService trackingService)
    {
        _trackingService = trackingService;
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

        if (!success)
        {
            return BadRequest("Geçersiz imza veya parametre.");
        }

        var emoji = isPositive ? "🎉" : "💡";
        var feedbackMessage = isPositive
            ? "Bülteni beğendiğiniz için çok teşekkür ederiz! Sizin için en kaliteli içerikleri hazırlamaya devam edeceğiz."
            : "Geri bildiriminiz için teşekkürler. Bültenlerimizi daha da geliştirmek ve ilgi alanlarınıza uygun hale getirmek için çalışacağız.";

        var html = $@"<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Geri Bildiriminiz Alındı | SUBMAIL</title>
    <link href='https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;600;700&display=swap' rel='stylesheet'>
    <style>
        body {{
            font-family: 'Plus Jakarta Sans', sans-serif;
            background: linear-gradient(135deg, #f0fdf4 0%, #e0f2fe 100%);
            margin: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            color: #1e293b;
        }}
        .card {{
            background: white;
            padding: 40px 32px;
            border-radius: 24px;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
            max-width: 440px;
            text-align: center;
            margin: 20px;
        }}
        .emoji {{
            font-size: 54px;
            margin-bottom: 16px;
        }}
        h1 {{
            font-size: 22px;
            font-weight: 700;
            margin: 0 0 12px;
            color: #0f172a;
        }}
        p {{
            font-size: 15px;
            line-height: 1.6;
            color: #64748b;
            margin: 0 0 24px;
        }}
        .btn {{
            display: inline-block;
            background: #2563eb;
            color: white;
            text-decoration: none;
            padding: 12px 24px;
            border-radius: 12px;
            font-weight: 600;
            font-size: 14px;
            transition: background 0.2s;
        }}
        .btn:hover {{
            background: #1d4ed8;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='emoji'>{emoji}</div>
        <h1>Geri Bildiriminiz Alındı!</h1>
        <p>{feedbackMessage}</p>
        <a href='/' class='btn'>Ana Sayfaya Dön</a>
    </div>
</body>
</html>";

        return Content(html, "text/html; charset=utf-8");
    }
}
