namespace EmailSubscriber.API.Models;

public class Campaign
{
    public int Id { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public int RecipientCount { get; set; }
    
    // Kategori ve Kapak Görseli (Arşiv & Web Görünümü için)
    public string? Category { get; set; }
    public string? CoverImageUrl { get; set; }
    
    // Navigation Properties
    public ICollection<CampaignRecipient> Recipients { get; set; } = new List<CampaignRecipient>();
    public ICollection<CampaignFeedback> Feedbacks { get; set; } = new List<CampaignFeedback>();
}
