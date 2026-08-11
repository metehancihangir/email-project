namespace EmailSubscriber.API.Models;

public class Campaign
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public int RecipientCount { get; set; }
    
    // Navigation Property
    public ICollection<CampaignRecipient> Recipients { get; set; } = new List<CampaignRecipient>();
}
