namespace EmailSubscriber.API.Services;

public interface ITrackingService
{
    Task<bool> TrackOpenAsync(int campaignId, int subscriberId);
    Task<(bool IsSuccess, string? OriginalUrl)> TrackClickAsync(string linkToken);
}
