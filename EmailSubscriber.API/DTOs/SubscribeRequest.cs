using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public record SubscribeRequest(
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    string Email,
    
    [MaxLength(100)]
    string? Name = null,
    
    [MaxLength(500)]
    string? Interests = null,

    // Botları engellemek için honeypot alanı.
    [MaxLength(100)]
    string? Website = null
);
