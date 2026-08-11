using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSubscriber.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _adminService.LoginAsync(request.Username, request.Password);
        
        if (token == null)
            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });

        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers([FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] bool? isConfirmed)
    {
        var subscribers = await _adminService.GetSubscribersAsync(search, isActive, isConfirmed);
        return Ok(subscribers);
    }

    [Authorize]
    [HttpPut("subscribers/{id}/deactivate")]
    public async Task<IActionResult> DeactivateSubscriber(int id)
    {
        var success = await _adminService.DeactivateSubscriberAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("subscribers/{id}")]
    public async Task<IActionResult> DeleteSubscriber(int id)
    {
        var success = await _adminService.DeleteSubscriberAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    [Authorize]
    [HttpGet("subscribers/growth")]
    public async Task<IActionResult> GetGrowthChart()
    {
        var data = await _adminService.GetGrowthChartAsync();
        return Ok(data);
    }

    [Authorize]
    [HttpPost("newsletter")]
    public async Task<IActionResult> SendNewsletter([FromBody] NewsletterSendRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (campaignId, recipientCount) = await _adminService.SendNewsletterAsync(request.Subject, request.HtmlBody);

        return Accepted(new { campaignId, recipientCount });
    }

    [Authorize]
    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaigns()
    {
        var campaigns = await _adminService.GetCampaignsAsync();
        return Ok(campaigns);
    }
}
