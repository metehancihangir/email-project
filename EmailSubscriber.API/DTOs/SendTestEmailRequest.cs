using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public class SendTestEmailRequest
{
    [Required(ErrorMessage = "Test e-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public required string TargetEmail { get; set; }

    [Required(ErrorMessage = "Konu başlığı zorunludur.")]
    public required string Subject { get; set; }

    [Required(ErrorMessage = "İçerik zorunludur.")]
    public required string HtmlBody { get; set; }

    public string? Category { get; set; }
    public string? CoverImageUrl { get; set; }
}
