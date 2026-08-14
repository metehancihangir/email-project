namespace EmailSubscriber.API.Services;

public interface ITrackingService
{
    string GenerateOpenSignature(int campaignId, int subscriberId);
    bool ValidateOpenSignature(int campaignId, int subscriberId, string? signature);
    Task<bool> TrackOpenAsync(int campaignId, int subscriberId, string? signature);
    Task<(bool IsSuccess, string? OriginalUrl)> TrackClickAsync(string linkToken);
}

