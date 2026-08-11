using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public record ResendRequest(
    [Required, EmailAddress] string Email
);
