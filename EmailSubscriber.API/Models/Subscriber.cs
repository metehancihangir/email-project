namespace EmailSubscriber.API.Models;

public class Subscriber
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string? Name { get; set; }
    public bool IsConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
    public string? ConfirmationToken { get; set; }
    public DateTime? ConfirmationTokenExpiresAt { get; set; }
    public string? UnsubscribeToken { get; set; }

    /// <summary>
    /// Faz 2 — Seçenek B (DB tabanlı rate-limiting).
    /// Son onay kodu isteğinin zamanı. NULL = hiç istek yapılmamış (ilk istekte cooldown uygulanmaz).
    /// 2 dakikalık cooldown kontrolü için kullanılır.
    /// </summary>
    public DateTime? LastCodeRequestedAt { get; set; }
}
