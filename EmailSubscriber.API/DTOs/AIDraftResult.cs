namespace EmailSubscriber.API.DTOs;

public class AIDraftResult
{
    public string Category { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public List<string> ReferenceUrls { get; set; } = new();
}
