namespace EmailSubscriber.API.Models;

public class TrackedLink
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int SubscriberId { get; set; }
    public required string OriginalUrl { get; set; }
    public required string LinkToken { get; set; }
    public DateTime? ClickedAt { get; set; }

    // Navigation Properties
    public Campaign? Campaign { get; set; }
    public Subscriber? Subscriber { get; set; }
}
