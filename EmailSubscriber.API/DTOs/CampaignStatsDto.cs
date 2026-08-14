namespace EmailSubscriber.API.DTOs;

public class CampaignStatsDto
{
    public int Sent { get; set; }
    public int Opened { get; set; }
    public double OpenRate { get; set; }
    public int Clicked { get; set; }
    public double ClickRate { get; set; }
    public int PositiveFeedbackCount { get; set; }
    public int TotalFeedbackCount { get; set; }
    public double PositiveFeedbackRate { get; set; }
}
