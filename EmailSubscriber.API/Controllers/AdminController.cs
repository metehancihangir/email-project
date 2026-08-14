using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("Api")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [AllowAnonymous]
    [EnableRateLimiting("Login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _adminService.LoginAsync(request.Username, request.Password);
        
        if (token == null)
            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] bool? isConfirmed, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var subscribers = await _adminService.GetSubscribersAsync(search, isActive, isConfirmed, page, pageSize);
        return Ok(subscribers);
    }

    [Authorize]
    [HttpPut("subscribers/{id}/deactivate")]
    public async Task<IActionResult> DeactivateSubscriber(int id)
    {
        var success = await _adminService.DeactivateSubscriberAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("subscribers/{id}")]
    public async Task<IActionResult> DeleteSubscriber(int id)
    {
        var success = await _adminService.DeleteSubscriberAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    [Authorize]
    [HttpGet("subscribers/growth")]
    public async Task<IActionResult> GetGrowthChart()
    {
        var data = await _adminService.GetGrowthChartAsync();
        return Ok(data);
    }

    [Authorize]
    [HttpPost("newsletter")]
    public async Task<IActionResult> SendNewsletter([FromBody] NewsletterSendRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (campaignId, recipientCount) = await _adminService.SendNewsletterAsync(request.Subject, request.HtmlBody);

        return Accepted(new { campaignId, recipientCount });
    }

    [Authorize]
    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns()
    {
        var campaigns = await _adminService.GetCampaignsAsync();
        return Ok(campaigns);
    }

    [Authorize]
    [HttpGet("campaigns/{id}/stats")]
    public async Task<IActionResult> GetCampaignStats(int id)
    {
        var stats = await _adminService.GetCampaignStatsAsync(id);
        if (stats == null) return NotFound();
        return Ok(stats);
    }

    [Authorize]
    [HttpGet("campaigns/{id}/content")]
    public async Task<IActionResult> GetCampaignContent(int id)
    {
        var content = await _adminService.GetCampaignContentAsync(id);
        if (content == null) return NotFound();
        return Ok(new { htmlBody = content });
    }

    [Authorize]
    [HttpPost("images/upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Dosya seçilmedi." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { message = "Sadece resim dosyaları yüklenebilir." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Dosya boyutu 5MB'dan küçük olmalıdır." });

        // Resim Yükleme Zafiyeti (Magic Number Bypass Koruması):
        // Sadece uzantıya güvenmek yerine dosyanın ilk byte'larını (header) okuyup gerçekten resim formatında olup olmadığını teyit ediyoruz.
        using (var stream = file.OpenReadStream())
        {
            var buffer = new byte[12];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            
            bool isJpg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            bool isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            bool isGif = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46;
            bool isWebp = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 && 
                          buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50; // WEBP header

            if (!isJpg && !isPng && !isGif && !isWebp)
            {
                return BadRequest(new { message = "Güvenlik İhlali: Seçilen dosya sahte bir görsel veya desteklenmeyen bir formattır." });
            }
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + ext;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var imageUrl = $"{baseUrl}/uploads/images/{uniqueFileName}";

        return Ok(new { url = imageUrl });
    }

    [Authorize]
    [HttpPost("trigger-ai")]
    public IActionResult TriggerAINewsletter([FromQuery] string? category, [FromServices] IServiceProvider serviceProvider)
    {
        var hostedServices = serviceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        var aiJob = hostedServices.OfType<EmailSubscriber.API.Services.Jobs.AINewsletterJob>().FirstOrDefault();

        if (aiJob == null)
            return StatusCode(500, new { message = "AINewsletterJob bulunamadı." });

        if (aiJob.IsRunning)
            return Conflict(new { message = "Yapay zeka bülten üretimi zaten devam ediyor. Lütfen önceki işlemin tamamlanmasını bekleyin." });

        _ = Task.Run(async () =>
        {
            try
            {
                await aiJob.GenerateAndSendNewsletterAsync(category);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<AdminController>>();
                logger.LogError(ex, "Yapay zeka bülteni manuel tetiklemesinde hata oluştu.");
            }
        });

        return Ok(new { message = "Yapay zeka bülten üretimi tetiklendi. Arka planda oluşturulup gönderilecek." });
    }

    [Authorize]
    [HttpDelete("images/{fileName}")]
    public IActionResult DeleteImage(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "images");
        var filePath = Path.Combine(uploadsFolder, safeFileName);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            return Ok(new { message = "Resim başarıyla silindi." });
        }

        return NotFound(new { message = "Resim bulunamadı." });
    }
}
