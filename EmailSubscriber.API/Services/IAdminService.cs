using EmailSubscriber.API.Models;

namespace EmailSubscriber.API.Services;

public interface IAdminService
{
    Task<string?> LoginAsync(string username, string password);
    Task<IEnumerable<Subscriber>> GetSubscribersAsync(string? search, bool? isActive, bool? isConfirmed);
    Task<bool> DeactivateSubscriberAsync(int id);
    Task<bool> DeleteSubscriberAsync(int id);
    Task<object> GetStatsAsync();
    Task<object> GetGrowthChartAsync();
}
