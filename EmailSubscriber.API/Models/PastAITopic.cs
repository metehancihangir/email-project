namespace EmailSubscriber.API.Models;

public class PastAITopic
{
    public int Id { get; set; }
    
    // Kategori (Örn: "Mitoloji", "Bilim", "Finans")
    public string Category { get; set; } = "";
    
    // Anlatılan konu (Örn: "Zeus ve Hera'nın hikayesi")
    public string TopicName { get; set; } = "";
    
    // Ne zaman gönderildiği
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
