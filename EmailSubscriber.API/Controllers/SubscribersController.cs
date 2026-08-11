using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscribersController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscribersController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    [HttpPost]
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
        var (isSuccess, message) = await _subscriberService.SubscribeAsync(request.Email, request.Name);

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

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (statusCode, message) = await _subscriberService.ResendConfirmationAsync(request.Email);

        if (statusCode == 202)
            return Accepted(new { message });
            
        return StatusCode(statusCode, new { message });
    }

    [HttpGet("unsubscribe/{token}")]
    public async Task<IActionResult> Unsubscribe(string token)
    {
        var (statusCode, message) = await _subscriberService.UnsubscribeAsync(token);
        
        if (statusCode == 200)
            return Ok(new { message });
            
        return StatusCode(statusCode, new { message });
    }
}
