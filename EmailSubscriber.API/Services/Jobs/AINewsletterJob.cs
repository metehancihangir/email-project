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
    private static readonly SemaphoreSlim _executionLock = new SemaphoreSlim(1, 1);

    public bool IsRunning => _executionLock.CurrentCount == 0;

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
                // Politika Cuma Akşam 20:00
                else if (localTime.DayOfWeek == DayOfWeek.Friday && localTime.Hour == 20)
                    categoryToRun = "Politika";

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

    public async Task<bool> GenerateAndSendNewsletterAsync(string? forceCategory = null)
    {
        if (!await _executionLock.WaitAsync(0))
        {
            _logger.LogWarning("Yapay zeka bülteni üretimi halihazırda çalışıyor. Yeni istek reddedildi.");
            return false;
        }

        try
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
                DayOfWeek.Wednesday => "Politika",
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

            // Mitoloji kategorisinde aynı karakterin tekrar seçilmesini önlemek için
            // geçmiş bülten başlıklarından karakter/tanrı adlarını çıkarıyoruz.
            string pastCharactersStr = string.Empty;
            if (category.Equals("Mitoloji", StringComparison.OrdinalIgnoreCase) && pastTopicsList.Any())
            {
                var characters = ExtractCharacterNames(pastTopicsList);
                if (characters.Any())
                    pastCharactersStr = string.Join(", ", characters);
            }

            _logger.LogInformation("Yapay Zeka'ya {Category} kategorisinde istek atılıyor. Geçmiş konular: {Past} | Geçmiş karakterler: {Chars}", category, pastTopicsStr, pastCharactersStr);

            // AI İçerik Üretimi
            string htmlContent = await aiService.GenerateNewsletterAsync(category, pastTopicsStr, pastCharactersStr);
            
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
                "politika" => "🌍",
                _ => "✨"
            };

            string subject = $"SUBMAIL {category} {categoryEmoji}: {extractedTopic} 🌟";

            // Konuya özel görseli al
            string? imageUrl = await aiService.GetImageUrlForTopicAsync(category, extractedTopic, htmlContent);
            // Bu kategoriyi isteyen aboneleri bul
            var subscribers = await context.Subscribers
                .Where(s => s.IsActive && s.IsConfirmed)
                .ToListAsync();

            // Sadece bu kategoriyi ilgi alanlarına eklemiş (Interests) aboneleri filtrele (Tam eşleşme)
            var targetSubscribers = subscribers
                .Where(s => !string.IsNullOrWhiteSpace(s.Interests) &&
                            s.Interests.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                       .Contains(category, StringComparer.OrdinalIgnoreCase))
                .ToList();
            
            foreach(var sub in subscribers)
            {
                _logger.LogInformation("Abone bulundu: {Email}, İlgi Alanları: {Interests}", sub.Email, sub.Interests);
            }

            _logger.LogInformation("{Count} aboneye gönderiliyor.", targetSubscribers.Count);

            await adminService.SendTargetedNewsletterAsync(subject, htmlContent, targetSubscribers.Select(s => s.Id).ToList(), category, imageUrl);

            // Konuyu veritabanına kaydet
            context.PastAITopics.Add(new PastAITopic
            {
                Category = category,
                TopicName = extractedTopic,
                GeneratedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            _logger.LogInformation("Bülten işlemi tamamlandı.");
            return true;
        }
        finally
        {
            _executionLock.Release();
        }
    }

    /// <summary>
    /// Mitoloji bülten başlıklarından tekrar eden karakterleri/tanrı isimlerini çıkarır.
    /// Örneğin: "Odin'in Kargaları" -> "Odin", "Zeus ve Hera" -> "Zeus", "Hera"
    /// </summary>
    private static List<string> ExtractCharacterNames(List<string> topics)
    {
        var characterCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Mitoloji dünyasında sık geçen bülten "dolgu" kelimeleri — bunlar karakter adı değil
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "bir", "ve", "ile", "ya", "da", "de", "ki", "veya", "the", "of", "and",
            "efsane", "efsanesi", "hikaye", "hikayesi", "mit", "miti", "tanrı", "tanrıça",
            "sırı", "sırrı", "dünyası", "kökeni", "kök", "efsanevi", "gücü", "laneti",
            "savaşçı", "kahramanları", "efendisi", "kralı", "kraliçesi", "dönemi"
        };

        foreach (var topic in topics)
        {
            // Apostrof ve tirelerle ayrılan kelimeleri böl
            var words = topic.Split(new[] { ' ', '-', '\'', ',', '.', ':', '!', '?' },
                                    StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                // Büyük harfle başlayan, en az 3 karakter, stop-word olmayan kelimeler karakter adı adaylarıdır
                if (word.Length >= 3 && char.IsUpper(word[0]) && !stopWords.Contains(word))
                {
                    characterCounts.TryGetValue(word, out int count);
                    characterCounts[word] = count + 1;
                }
            }
        }

        // En az 1 kez geçen adayları döndür (bir kez bile görünmesi bile tekrar sayılır)
        return characterCounts
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .Take(15) // Listeyi makul tutuyoruz
            .ToList();
    }
}
