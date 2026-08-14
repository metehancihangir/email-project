namespace EmailSubscriber.API.Models;

public class CampaignFeedback
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int SubscriberId { get; set; }
    public bool IsPositive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Campaign? Campaign { get; set; }
    public Subscriber? Subscriber { get; set; }
}
