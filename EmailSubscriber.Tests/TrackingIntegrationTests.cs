using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EmailSubscriber.API;
using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace EmailSubscriber.Tests;

public class TrackingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TrackingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Optionally replace DB for integration testing 
                // But we'll just use the existing test db configuration
            });
        });
        
        // Don't follow redirects automatically so we can assert the 302
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task TrackOpen_ShouldSetOpenedAtAndReturnGif()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sub = new Subscriber { Email = $"open_{Guid.NewGuid()}@test.com", Name = "Open", IsConfirmed = true, IsActive = true };
        context.Subscribers.Add(sub);
        
        var camp = new Campaign { Subject = "Test Open", HtmlBody = "Body", RecipientCount = 1 };
        context.Campaigns.Add(camp);
        await context.SaveChangesAsync();

        var recipient = new CampaignRecipient { CampaignId = camp.Id, SubscriberId = sub.Id, Status = "sent" };
        context.CampaignRecipients.Add(recipient);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/track/open/{camp.Id}/{sub.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/gif", response.Content.Headers.ContentType?.MediaType);
        
        // Assert Cache-Control
        var cacheControl = response.Headers.CacheControl;
        Assert.True(cacheControl?.NoCache);

        // Verify DB update
        await context.Entry(recipient).ReloadAsync();
        Assert.NotNull(recipient.OpenedAt);
    }

    [Fact]
    public async Task TrackClick_ShouldRedirectAndSetClickedAt()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var sub = new Subscriber { Email = $"click_{Guid.NewGuid()}@test.com", Name = "Click", IsConfirmed = true, IsActive = true };
        context.Subscribers.Add(sub);
        
        var camp = new Campaign { Subject = "Test Click", HtmlBody = "Body", RecipientCount = 1 };
        context.Campaigns.Add(camp);
        await context.SaveChangesAsync();

        var recipient = new CampaignRecipient { CampaignId = camp.Id, SubscriberId = sub.Id, Status = "sent" };
        context.CampaignRecipients.Add(recipient);
        await context.SaveChangesAsync();

        var linkToken = System.Guid.NewGuid().ToString("N");
        var trackedLink = new TrackedLink 
        { 
            CampaignId = camp.Id, 
            SubscriberId = sub.Id, 
            OriginalUrl = "https://example.com/test", 
            LinkToken = linkToken 
        };
        context.TrackedLinks.Add(trackedLink);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/track/click/{linkToken}");

        // Assert
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("https://example.com/test", response.Headers.Location?.ToString());

        // Verify DB update
        await context.Entry(trackedLink).ReloadAsync();
        Assert.NotNull(trackedLink.ClickedAt);

        await context.Entry(recipient).ReloadAsync();
        Assert.NotNull(recipient.ClickedAt);
    }

    [Fact]
    public async Task TrackClick_InvalidToken_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/track/click/invalid_token_123");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
