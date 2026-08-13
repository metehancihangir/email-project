using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using EmailSubscriber.API.Queue;
using EmailSubscriber.API.Services.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services.Jobs;

public class AINewsletterJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AINewsletterJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);

    public AINewsletterJob(IServiceProvider serviceProvider, ILogger<AINewsletterJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AINewsletterJob başlatıldı. Belirli gün ve saatlerde AI bülteni oluşturulacak.");

        // Hangi kategorinin o gün çalıştırıldığını takip etmek için
        var lastRunTracker = new Dictionary<string, DateTime>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Sunucu UTC çalışsa bile Türkiye saatine göre işlem yapalım
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);

                string? categoryToRun = null;

                // Finans Cumartesi Sabah 10:00
                if (localTime.DayOfWeek == DayOfWeek.Saturday && localTime.Hour == 10)
                    categoryToRun = "Finans";
                // Bilim Cumartesi Akşam 20:00
                else if (localTime.DayOfWeek == DayOfWeek.Saturday && localTime.Hour == 20)
                    categoryToRun = "Bilim";
                // Mitoloji Pazar Sabah 10:00
                else if (localTime.DayOfWeek == DayOfWeek.Sunday && localTime.Hour == 10)
                    categoryToRun = "Mitoloji";

                if (categoryToRun != null)
                {
                    bool alreadyRanToday = lastRunTracker.TryGetValue(categoryToRun, out var lastRun) 
                                           && lastRun.Date == localTime.Date;

                    if (!alreadyRanToday)
                    {
                        _logger.LogInformation("{Category} bülteni için zaman geldi. Tetikleniyor... (Yerel Saat: {Time})", categoryToRun, localTime);
                        await GenerateAndSendNewsletterAsync(categoryToRun);
                        
                        // Gün içinde bir daha tetiklenmemesi için kaydet
                        lastRunTracker[categoryToRun] = localTime;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yapay zeka bülteni oluşturulurken hata meydana geldi.");
            }

            // Her 15 dakikada bir kontrol eder, saat dilimi yakalandığında bülteni gönderir.
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    // Bu metot ayrıca manual test için public yapılabilir (eğer başka bir servise alırsak). 
    // Ancak BackgroundService olduğu için doğrudan inject edilemez, sadece provider ile çağırılabilir.
    // Şimdilik internal logic olarak bırakıyoruz.
    public async Task GenerateAndSendNewsletterAsync(string? forceCategory = null)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();
        var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();

        // Günün kategorisini seç
        string category = forceCategory ?? DateTime.UtcNow.DayOfWeek switch
        {
            DayOfWeek.Monday or DayOfWeek.Thursday => "Finans",
            DayOfWeek.Tuesday or DayOfWeek.Friday => "Bilim",
            _ => "Mitoloji"
        };

        // Geçmişte bu kategoride yazılanları bul
        var pastTopicsList = await context.PastAITopics
            .Where(p => p.Category == category)
            .OrderByDescending(p => p.GeneratedAt)
            .Take(10) // Son 10 konu yeterli
            .Select(p => p.TopicName)
            .ToListAsync();

        string pastTopicsStr = pastTopicsList.Any() ? string.Join(", ", pastTopicsList) : "Yok";

        _logger.LogInformation("Yapay Zeka'ya {Category} kategorisinde istek atılıyor. Geçmiş konular: {Past}", category, pastTopicsStr);

        // AI İçerik Üretimi
        string htmlContent = await aiService.GenerateNewsletterAsync(category, pastTopicsStr);
        
        // Güvenlik: Gemini bazen HTML yerine Markdown (**) kullanmakta israr edebiliyor.
        // E-posta içerisinde ** (çift yıldız) görünmemesi için bunları <b> etiketine çeviriyoruz.
        htmlContent = System.Text.RegularExpressions.Regex.Replace(htmlContent, @"\*\*(.*?)\*\*", "<b>$1</b>");

        // XSS (Cross-Site Scripting) Koruması: Yapay zekanın zararlı bir script üretme ihtimaline karşı
        // HTML içeriğini HtmlSanitizer ile temizliyoruz (Sadece güvenli etiketler kalır)
        var sanitizer = new Ganss.Xss.HtmlSanitizer();
        htmlContent = sanitizer.Sanitize(htmlContent);

        // AI'ın ürettiği HTML'den <h2> başlığını yakalayarak konuyu bulalım
        string extractedTopic = "Yeni Bülten Konusu";
        var match = System.Text.RegularExpressions.Regex.Match(htmlContent, @"<h2>(.*?)</h2>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            extractedTopic = match.Groups[1].Value;
            extractedTopic = System.Text.RegularExpressions.Regex.Replace(extractedTopic, "<.*?>", string.Empty).Trim();
        }

        string categoryEmoji = category.ToLower() switch
        {
            "mitoloji" => "🏛️",
            "finans" => "📈",
            "bilim" => "🔬",
            _ => "✨"
        };

        string subject = $"SUBMAIL {category} {categoryEmoji}: {extractedTopic} 🌟";

        // Konuya özel görseli al
        string? imageUrl = await aiService.GetImageUrlForTopicAsync(category, extractedTopic);
        // Bu kategoriyi isteyen aboneleri bul
        var subscribers = await context.Subscribers
            .Where(s => s.IsActive && s.IsConfirmed)
            .ToListAsync();

        // Sadece bu kategoriyi ilgi alanlarına eklemiş (Interests) aboneleri filtrele
        var targetSubscribers = subscribers
            .Where(s => !string.IsNullOrEmpty(s.Interests) && s.Interests.Contains(category))
            .ToList();
        
        foreach(var sub in subscribers)
        {
            _logger.LogInformation("Abone bulundu: {Email}, İlgi Alanları: {Interests}", sub.Email, sub.Interests);
        }

        _logger.LogInformation("{Count} aboneye gönderiliyor.", targetSubscribers.Count);

        string finalHtml = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
            <div style='text-align: center; margin-bottom: 20px;'>
                <img src='{imageUrl}' alt='{category}' style='max-width: 100%; border-radius: 8px;' />
            </div>
            {htmlContent}
            <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;' />
            <p style='font-size: 12px; color: #999; text-align: center;'>Bu bülten SUBMAIL AI tarafından otomatik oluşturulmuştur.</p>
        </div>";

        await adminService.SendTargetedNewsletterAsync(subject, finalHtml, targetSubscribers.Select(s => s.Id).ToList());

        // Konuyu veritabanına kaydet
        context.PastAITopics.Add(new PastAITopic
        {
            Category = category,
            TopicName = extractedTopic,
            GeneratedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        _logger.LogInformation("Bülten işlemi tamamlandı.");
    }
}
