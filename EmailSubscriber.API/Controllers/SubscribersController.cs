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
}
