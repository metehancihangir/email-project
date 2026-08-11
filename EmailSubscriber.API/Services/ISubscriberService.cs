namespace EmailSubscriber.API.Services;

public interface ISubscriberService
{
    Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name);
}
