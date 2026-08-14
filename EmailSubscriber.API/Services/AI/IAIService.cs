using EmailSubscriber.API.DTOs;

namespace EmailSubscriber.API.Services.AI;

public interface IAIService
{
    Task<string> GenerateNewsletterAsync(string category, string pastTopics);
    Task<string?> GetImageUrlForTopicAsync(string category, string topic, string htmlContent = "");
    Task<AIDraftResult> GenerateNewsletterDraftAsync(string category, string? pastTopics = null);
}
