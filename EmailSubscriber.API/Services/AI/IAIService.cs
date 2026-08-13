namespace EmailSubscriber.API.Services.AI;

public interface IAIService
{
    Task<string> GenerateNewsletterAsync(string category, string pastTopics);
    Task<string?> GetImageUrlForTopicAsync(string category, string topic, string htmlContent = "");
}
