using EmailSubscriber.API.Models;

namespace EmailSubscriber.API.Services;

public interface IAdminService
{
    Task<string?> LoginAsync(string username, string password);
    Task<EmailSubscriber.API.DTOs.PagedResult<EmailSubscriber.API.DTOs.SubscriberDto>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed, int page = 1, int pageSize = 20);
    Task<bool> DeactivateSubscriberAsync(int id);
    Task<bool> DeleteSubscriberAsync(int id);
    Task<object> GetStatsAsync();
    Task<object> GetGrowthChartAsync();
    
    // Newsletter & Campaigns
    Task<(int campaignId, int recipientCount)> SendNewsletterAsync(string subject, string htmlBody);
    Task<IEnumerable<EmailSubscriber.API.DTOs.CampaignDto>> GetCampaignsAsync();
    Task<EmailSubscriber.API.DTOs.CampaignStatsDto?> GetCampaignStatsAsync(int id);
}
