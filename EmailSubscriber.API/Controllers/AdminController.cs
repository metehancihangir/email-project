using EmailSubscriber.API.Data;
using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Services;
using EmailSubscriber.API.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Timeouts;

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

        int campaignId;
        int recipientCount;

        if (request.TargetSubscriberIds != null && request.TargetSubscriberIds.Any())
        {
            (campaignId, recipientCount) = await _adminService.SendTargetedNewsletterAsync(
                request.Subject, 
                request.HtmlBody, 
                request.TargetSubscriberIds, 
                request.Category, 
                request.CoverImageUrl);
        }
        else
        {
            (campaignId, recipientCount) = await _adminService.SendNewsletterAsync(
                request.Subject, 
                request.HtmlBody, 
                request.Category, 
                request.CoverImageUrl);
        }

        return Accepted(new { campaignId, recipientCount });
    }

    [Authorize]
    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var success = await _adminService.SendTestEmailAsync(
            request.TargetEmail, 
            request.Subject, 
            request.HtmlBody, 
            request.Category, 
            request.CoverImageUrl);

        if (!success)
            return StatusCode(500, new { message = "Test e-postası gönderilemedi." });

        return Ok(new { message = $"Test e-postası başarıyla kuyruğa alındı: {request.TargetEmail}" });
    }

    [Authorize]
    [HttpPost("generate-draft")]
    [RequestTimeout(120_000)] // AI taslak üretimi uzun sürebileceğinden 120 saniye timeout
    public async Task<IActionResult> GenerateDraft([FromQuery] string? category, [FromServices] IAIService aiService, [FromServices] AppDbContext context)
    {
        var selectedCategory = category ?? "Mitoloji";

        try
        {
            var pastTopicsList = await context.PastAITopics
                .Where(p => p.Category == selectedCategory)
                .OrderByDescending(p => p.GeneratedAt)
                .Take(10)
                .Select(p => p.TopicName)
                .ToListAsync();

            string pastTopicsStr = pastTopicsList.Any() ? string.Join(", ", pastTopicsList) : "Yok";

            // Mitoloji kategorisinde aynı karakterin tekrar seçilmesini önlemek için karakter adlarını çıkar
            string pastCharactersStr = string.Empty;
            if (selectedCategory.Equals("Mitoloji", StringComparison.OrdinalIgnoreCase) && pastTopicsList.Any())
            {
                var characters = ExtractMythologyCharacters(pastTopicsList);
                if (characters.Any())
                    pastCharactersStr = string.Join(", ", characters);
            }

            var draft = await aiService.GenerateNewsletterDraftAsync(selectedCategory, pastTopicsStr);
            return Ok(draft);
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                message = "Yapay zeka taslak üretimi başarısız oldu. Lütfen API anahtarınızı ve Gemini model erişiminizi kontrol edip tekrar deneyin."
            });
        }
    }

    /// <summary>
    /// Mitoloji bülten başlıklarından karakter adlarını çıkarır (AdminController'a özel kopya).
    /// </summary>
    private static List<string> ExtractMythologyCharacters(List<string> topics)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bir", "ve", "ile", "ya", "da", "de", "ki", "veya", "the", "of", "and",
            "efsane", "efsanesi", "hikaye", "hikayesi", "mit", "miti", "tanrı", "tanrıça",
            "sırı", "sırrı", "dünyası", "kökeni", "kök", "efsanevi", "gücü", "laneti"
        };

        return topics
            .SelectMany(t => t.Split(new[] { ' ', '-', '\'', ',', '.', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length >= 3 && char.IsUpper(w[0]) && !stopWords.Contains(w))
            .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(15)
            .ToList();
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

        // Resim Yükleme Zafiyeti (Magic Number Bypass Koruması)
        using (var stream = file.OpenReadStream())
        {
            var buffer = new byte[12];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            
            bool isJpg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            bool isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            bool isGif = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46;
            bool isWebp = buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 && 
                          buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50;

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
