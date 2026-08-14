namespace EmailSubscriber.API.DTOs;

public class PublicNewsletterDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string? Category { get; set; }
    public string? CoverImageUrl { get; set; }
    public DateTime SentAt { get; set; }
    public int LikesCount { get; set; }
}
