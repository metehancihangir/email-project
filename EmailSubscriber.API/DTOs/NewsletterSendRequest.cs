using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public class NewsletterSendRequest
{
    [Required(ErrorMessage = "Konu (Subject) alanı zorunludur.")]
    [StringLength(255)]
    public required string Subject { get; set; }

    [Required(ErrorMessage = "E-posta içeriği (HtmlBody) zorunludur.")]
    public required string HtmlBody { get; set; }
}
