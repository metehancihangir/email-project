namespace EmailSubscriber.API.Queue;

public record EmailJob(string To, string? ToName, string Subject, string HtmlBody, int? CampaignRecipientId = null);
