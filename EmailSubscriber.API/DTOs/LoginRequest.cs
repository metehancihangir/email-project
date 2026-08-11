using System.ComponentModel.DataAnnotations;

namespace EmailSubscriber.API.DTOs;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);
