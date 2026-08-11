namespace EmailSubscriber.API.Services;

public interface ISubscriberService
{
    Task<(bool IsSuccess, string Message)> SubscribeAsync(string email, string? name);
    Task<(int StatusCode, string Message)> ConfirmSubscriberAsync(string token);
    Task<(int StatusCode, string Message)> ResendConfirmationAsync(string email);
    Task<(int StatusCode, string Message)> UnsubscribeAsync(string token);
}
