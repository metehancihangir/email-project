using EmailSubscriber.API.DTOs;

namespace EmailSubscriber.API.Services;

public interface ISubscriberService
{
    Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name, string? interests = null);
    Task<(int StatusCode, string Message)> ConfirmSubscriberAsync(string token);
    Task<(int StatusCode, string Message)> ResendConfirmationAsync(string email);
    Task<ResendCodeResult> ResendConfirmationWithRateLimitAsync(string email);

    /// <summary>
    /// Faz 2 / 2.5 — Sayfa yenileme koruması için durum endpoint'i.
    /// LastCodeRequestedAt üzerinden nextAllowedAt hesaplar.
    /// </summary>
    Task<(bool IsDisabled, DateTime? NextAllowedAt)> GetResendStatusAsync(string email);

    Task<(int StatusCode, string Message, string? Email)> ValidateUnsubscribeTokenAsync(string token);
    Task<(int StatusCode, string Message)> UnsubscribeAsync(string token);
    
    Task<(int StatusCode, string Message, string? Interests)> GetPreferencesAsync(string token);
    Task<(int StatusCode, string Message)> UpdatePreferencesAsync(string token, string? interests);

    // Ziyaretçiler için Bülten Arşivi
    Task<PagedResult<PublicNewsletterDto>> GetPublicArchiveAsync(string? category, string? search, int page = 1, int pageSize = 12);
    Task<PublicNewsletterDto?> GetPublicNewsletterByIdAsync(int id);
}
