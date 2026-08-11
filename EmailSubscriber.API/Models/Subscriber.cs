namespace EmailSubscriber.API.Models;

public class Subscriber
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string? Name { get; set; }
    public bool IsConfirmed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAt { get; set; }
    public string? ConfirmationToken { get; set; }
    public DateTime? ConfirmationTokenExpiresAt { get; set; }
}
