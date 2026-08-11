namespace EmailSubscriber.API.DTOs;

public class CampaignDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public int RecipientCount { get; set; }
}
