namespace EmailSubscriber.API.DTOs;

/// <summary>
/// Faz 2 — DB tabanlı (Seçenek B) rate-limiting için resend sonuç nesnesi.
/// </summary>
public record ResendCodeResult(
    bool IsSuccess,
    bool IsRateLimited,
    bool IsNotFound,
    /// <summary>Rate-limited durumunda kalan saniye (429 Retry-After için)</summary>
    int? RetryAfterSeconds,
    /// <summary>Başarı durumunda bir sonraki isteğe izin verilen zaman</summary>
    DateTime? NextAllowedAt,
    string? Message
)
{
    public static ResendCodeResult Success(DateTime nextAllowedAt) =>
        new(true, false, false, null, nextAllowedAt, "Yeni onay e-postası gönderildi.");

    public static ResendCodeResult RateLimited(int retryAfterSeconds, DateTime nextAllowedAt) =>
        new(false, true, false, retryAfterSeconds, nextAllowedAt, "Lütfen 2 dakika bekleyin.");

    /// <summary>Email enumeration koruması: NotFound ve Confirmed aynı yanıtı döner.</summary>
    public static ResendCodeResult GenericInvalid() =>
        new(false, false, true, null, null, "Geçersiz istek.");
}
