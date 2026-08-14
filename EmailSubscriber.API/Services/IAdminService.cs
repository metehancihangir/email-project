using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Models;

namespace EmailSubscriber.API.Services;

public interface IAdminService
{
    Task<string?> LoginAsync(string username, string password);
    Task<PagedResult<SubscriberDto>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed, int page = 1, int pageSize = 20);
    Task<bool> DeactivateSubscriberAsync(int id);
    Task<bool> DeleteSubscriberAsync(int id);
    Task<object> GetStatsAsync();
    Task<object> GetGrowthChartAsync();
    
    // Newsletter & Campaigns
    Task<(int campaignId, int recipientCount)> SendNewsletterAsync(string subject, string htmlBody, string? category = null, string? coverImageUrl = null);
    Task<(int campaignId, int recipientCount)> SendTargetedNewsletterAsync(string subject, string htmlBody, List<int> subscriberIds, string? category = null, string? coverImageUrl = null);
    Task<bool> SendTestEmailAsync(string targetEmail, string subject, string htmlBody, string? category = null, string? coverImageUrl = null);
    Task<IEnumerable<CampaignDto>> GetCampaignsAsync();
    Task<CampaignStatsDto?> GetCampaignStatsAsync(int id);
    Task<string?> GetCampaignContentAsync(int id);
}
