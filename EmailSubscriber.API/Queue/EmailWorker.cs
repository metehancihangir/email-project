using EmailSubscriber.API.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Queue;

public class EmailWorker : BackgroundService
{
    private readonly IEmailQueueService _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailWorker> _logger;
    private const int MaxRetry = 3;

    public EmailWorker(IEmailQueueService queue, IServiceProvider serviceProvider, ILogger<EmailWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("EmailWorker başlatıldı. Kuyruk dinleniyor...");

        await foreach (var job in _queue.DequeueAllAsync(ct))
        {
            _logger.LogInformation("Kuyruktan yeni e-posta görevi alındı: Kime: {To}", job.To);

            for (int attempt = 1; attempt <= MaxRetry; attempt++)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendAsync(job.To, job.ToName, job.Subject, job.HtmlBody);
                    
                    _logger.LogInformation("E-posta başarıyla gönderildi: {To}", job.To);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "E-posta gönderimi başarısız (Deneme {Attempt}/{Max}): {To}", attempt, MaxRetry, job.To);
                    
                    if (attempt < MaxRetry)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s
                        _logger.LogInformation("{Delay} saniye sonra tekrar denenecek...", delay.TotalSeconds);
                        await Task.Delay(delay, ct);
                    }
                    else
                    {
                        _logger.LogError("E-posta maksimum deneme sayısına ulaştı ve gönderilemedi: {To}", job.To);
                    }
                }
            }
        }
    }
}
