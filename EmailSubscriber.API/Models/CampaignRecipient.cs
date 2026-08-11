namespace EmailSubscriber.API.Models;

public class CampaignRecipient
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int SubscriberId { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // "sent", "failed", "pending"
    public string Status { get; set; } = "pending";

    // Navigation Properties
    public Campaign? Campaign { get; set; }
    public Subscriber? Subscriber { get; set; }
}
