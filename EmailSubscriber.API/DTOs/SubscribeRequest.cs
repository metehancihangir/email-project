using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public record SubscribeRequest(
    [Required]
    [EmailAddress]
    string Email,
    
    [MaxLength(100)]
    string? Name,
    
    // Botları engellemek için honeypot alanı.
    string? Website
);
