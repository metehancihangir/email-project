namespace EmailSubscriber.API.DTOs;

public class CampaignDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public int RecipientCount { get; set; }
    public string? Category { get; set; }
    public string? CoverImageUrl { get; set; }
    
    // Analitik
    public int OpenedCount { get; set; }
    public double OpenRate { get; set; }
    public int ClickedCount { get; set; }
    public double ClickRate { get; set; }

    // Geri Bildirim (👍 / 👎)
    public int PositiveFeedbackCount { get; set; }
    public int TotalFeedbackCount { get; set; }
    public double PositiveFeedbackRate { get; set; }
}
