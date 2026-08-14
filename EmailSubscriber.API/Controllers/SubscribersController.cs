using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("Api")]
public class SubscribersController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscribersController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    [HttpPost]
    [EnableRateLimiting("Subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        // 1. Honeypot kontrolü (Bot Koruması)
        if (!string.IsNullOrEmpty(request.Website))
        {
            // Honeypot alanı doldurulmuşsa bu bir bottur.
            return BadRequest(new { message = "Geçersiz istek." });
        }

        // 2. ModelState validasyonu (Email adresi geçerliliği DTO üzerinde [EmailAddress] ile sağlanır)
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // 3. Servis katmanını çağır
        var (isSuccess, message) = await _subscriberService.SubscribeAsync(request.Email, request.Name, request.Interests);

        if (!isSuccess)
        {
            if (message == "Conflict")
            {
                return Conflict(new { message = "Bu e-posta adresi zaten kayıtlı." });
            }

            return BadRequest(new { message });
        }

        // Başarılı (202 Accepted)
        return Accepted(new { message });
    }

    [HttpGet("confirm/{token}")]
    public async Task<IActionResult> Confirm(string token)
    {
        var (statusCode, message) = await _subscriberService.ConfirmSubscriberAsync(token);
        
        return statusCode switch
        {
            200 => Ok(new { message }),
            404 => NotFound(new { message }),
            410 => StatusCode(410, new { message }),
            _ => StatusCode(statusCode, new { message })
        };
    }

    /// <summary>
    /// Geriye dönük uyumluluk için korunan endpoint. Artık rate-limiting ve cooldown kontrollerine tabi tutulur.
    /// </summary>
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _subscriberService.ResendConfirmationWithRateLimitAsync(request.Email);

        if (result.IsSuccess)
        {
            return Accepted(new
            {
                message = result.Message,
                nextAllowedAt = result.NextAllowedAt?.ToString("o")
            });
        }

        if (result.IsRateLimited)
        {
            Response.Headers["Retry-After"] = result.RetryAfterSeconds!.Value.ToString();
            return StatusCode(429, new
            {
                message = result.Message,
                retryAfterSeconds = result.RetryAfterSeconds,
                nextAllowedAt = result.NextAllowedAt?.ToString("o")
            });
        }

        return BadRequest(new { message = result.Message });
    }

    // ─── Faz 2: Rate-Limited Resend Endpoints ────────────────────────────────

    /// <summary>
    /// Faz 2 / 2.4.x — Rate-limited onay kodu tekrar gönderme.
    /// 200 OK: Kod gönderildi + nextAllowedAt
    /// 429 Too Many Requests: Cooldown aktif + Retry-After header + retryAfterSeconds
    /// 400 Bad Request: Geçersiz email (enumeration-safe generic mesaj)
    /// </summary>
    [HttpPost("resend-confirmation-v2")]
    public async Task<IActionResult> ResendConfirmationWithRateLimit([FromBody] ResendRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _subscriberService.ResendConfirmationWithRateLimitAsync(request.Email);

        if (result.IsSuccess)
        {
            // 2.4.2: 200 OK + nextAllowedAt
            return Ok(new
            {
                message = result.Message,
                nextAllowedAt = result.NextAllowedAt?.ToString("o") // ISO 8601
            });
        }

        if (result.IsRateLimited)
        {
            // 2.4.3: 429 + retryAfterSeconds
            // 2.4.4: Standart Retry-After HTTP header
            Response.Headers["Retry-After"] = result.RetryAfterSeconds!.Value.ToString();
            return StatusCode(429, new
            {
                message = result.Message,
                retryAfterSeconds = result.RetryAfterSeconds,
                nextAllowedAt = result.NextAllowedAt?.ToString("o")
            });
        }

        // 2.4.5: Email enumeration koruması — generic 400
        return BadRequest(new { message = result.Message });
    }

    /// <summary>
    /// Faz 2 / 2.5.x — Sayfa yenileme durumunda client-side state senkronizasyonu.
    /// Frontend component mount olduğunda LastCodeRequestedAt'ten nextAllowedAt hesaplar.
    /// </summary>
    [HttpGet("resend-status")]
    public async Task<IActionResult> GetResendStatus([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email parametresi gerekli." });

        var (isDisabled, nextAllowedAt) = await _subscriberService.GetResendStatusAsync(email);

        return Ok(new
        {
            isDisabled,
            nextAllowedAt = nextAllowedAt?.ToString("o") // ISO 8601, null ise cooldown yok
        });
    }

    /// <summary>
    /// Unsubscribe token doğrulama (Salt-Okunur). E-posta güvenlik tarayıcılarının
    /// HTTP GET ön taramasında kullanıcının yanlışlıkla abonelikten çıkmasını engeller.
    /// </summary>
    [HttpGet("unsubscribe/{token}")]
    public async Task<IActionResult> CheckUnsubscribeToken(string token)
    {
        var (statusCode, message, email) = await _subscriberService.ValidateUnsubscribeTokenAsync(token);
        
        if (statusCode == 200)
            return Ok(new { message, email });
            
        return StatusCode(statusCode, new { message });
    }

    /// <summary>
    /// Kullanıcının butona tıklamasıyla abonelikten çıkma işlemini gerçekleştiren endpoint (State-Changing POST).
    /// </summary>
    [HttpPost("unsubscribe/{token}")]
    public async Task<IActionResult> Unsubscribe(string token)
    {
        var (statusCode, message) = await _subscriberService.UnsubscribeAsync(token);
        
        if (statusCode == 200)
            return Ok(new { message });
            
        return StatusCode(statusCode, new { message });
    }

    [HttpGet("preferences/{token}")]
    public async Task<IActionResult> GetPreferences(string token)
    {
        var (statusCode, message, interests) = await _subscriberService.GetPreferencesAsync(token);

        if (statusCode == 200)
            return Ok(new { message, interests });

        return StatusCode(statusCode, new { message });
    }

    [HttpPut("preferences/{token}")]
    public async Task<IActionResult> UpdatePreferences(string token, [FromBody] UpdatePreferencesRequest request)
    {
        var (statusCode, message) = await _subscriberService.UpdatePreferencesAsync(token, request.Interests);

        if (statusCode == 200)
            return Ok(new { message });

        return StatusCode(statusCode, new { message });
    }

    [HttpGet("archive")]
    public async Task<IActionResult> GetArchive([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _subscriberService.GetPublicArchiveAsync(category, search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("archive/{id}")]
    public async Task<IActionResult> GetArchiveDetail(int id)
    {
        var result = await _subscriberService.GetPublicNewsletterByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "Bülten bulunamadı." });

        return Ok(result);
    }
}

