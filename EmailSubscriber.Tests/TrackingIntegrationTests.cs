using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EmailSubscriber.API;
using EmailSubscriber.API.Data;
using EmailSubscriber.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace EmailSubscriber.Tests;

public class TrackingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TrackingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Smtp:User", "" },
                    { "Gemini:ApiKey", "" },
                    { "Gemini:FallbackApiKey", "" },
                    { "Jwt:Secret", TestConfiguration.JwtSecret },
                    { "ConnectionStrings:Default", "Server=localhost;Database=dummy;Uid=root;Pwd=;" }
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(d => d.ServiceType.Name.Contains("DbContextOptions")).ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTrackingTesting");
                });
            });
        });
    }
        
    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task TrackOpen_WithValidSignature_ShouldSetOpenedAtAndReturnGif()
    {
        // Arrange
        int campId;
        int subId;
        int recipientId;
        string sig;

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var trackingService = scope.ServiceProvider.GetRequiredService<EmailSubscriber.API.Services.ITrackingService>();

            var sub = new Subscriber { Email = $"open_{Guid.NewGuid()}@test.com", Name = "Open", IsConfirmed = true, IsActive = true };
            context.Subscribers.Add(sub);
            
            var camp = new Campaign { Subject = "Test Open", HtmlBody = "Body", RecipientCount = 1 };
            context.Campaigns.Add(camp);
            await context.SaveChangesAsync();

            var recipient = new CampaignRecipient { CampaignId = camp.Id, SubscriberId = sub.Id, Status = "sent" };
            context.CampaignRecipients.Add(recipient);
            await context.SaveChangesAsync();

            sig = trackingService.GenerateOpenSignature(camp.Id, sub.Id);
            campId = camp.Id;
            subId = sub.Id;
            recipientId = recipient.Id;
        }

        // Act
        var client = CreateClient();
        var response = await client.GetAsync($"/api/track/open/{campId}/{subId}?sig={sig}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/gif", response.Content.Headers.ContentType?.MediaType);
        
        // Assert Cache-Control
        var cacheControl = response.Headers.CacheControl;
        Assert.True(cacheControl?.NoCache);

        // Verify DB update
        using (var verifyScope = _factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedRecipient = await verifyDb.CampaignRecipients.FirstOrDefaultAsync(r => r.Id == recipientId);
            Assert.NotNull(updatedRecipient?.OpenedAt);
        }
    }

    [Fact]
    public async Task TrackOpen_WithoutSignature_ShouldNotSetOpenedAt()
    {
        // Arrange
        int campId;
        int subId;
        int recipientId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sub = new Subscriber { Email = $"open_tamper_{Guid.NewGuid()}@test.com", Name = "Tamper", IsConfirmed = true, IsActive = true };
            context.Subscribers.Add(sub);
            
            var camp = new Campaign { Subject = "Test Tamper", HtmlBody = "Body", RecipientCount = 1 };
            context.Campaigns.Add(camp);
            await context.SaveChangesAsync();

            var recipient = new CampaignRecipient { CampaignId = camp.Id, SubscriberId = sub.Id, Status = "sent" };
            context.CampaignRecipients.Add(recipient);
            await context.SaveChangesAsync();

            campId = camp.Id;
            subId = sub.Id;
            recipientId = recipient.Id;
        }

        // Act - no sig
        var client = CreateClient();
        var response = await client.GetAsync($"/api/track/open/{campId}/{subId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("image/gif", response.Content.Headers.ContentType?.MediaType);

        // Verify DB is NOT updated (tamper prevented)
        using (var verifyScope = _factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var checkRecipient = await verifyDb.CampaignRecipients.FirstOrDefaultAsync(r => r.Id == recipientId);
            Assert.Null(checkRecipient?.OpenedAt);
        }
    }

    [Fact]
    public async Task TrackClick_ShouldRedirectAndSetClickedAt()
    {
        // Arrange
        int recipientId;
        int trackedLinkId;
        var linkToken = System.Guid.NewGuid().ToString("N");
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var sub = new Subscriber { Email = $"click_{Guid.NewGuid()}@test.com", Name = "Click", IsConfirmed = true, IsActive = true };
            context.Subscribers.Add(sub);
            
            var camp = new Campaign { Subject = "Test Click", HtmlBody = "Body", RecipientCount = 1 };
            context.Campaigns.Add(camp);
            await context.SaveChangesAsync();

            var recipient = new CampaignRecipient { CampaignId = camp.Id, SubscriberId = sub.Id, Status = "sent" };
            context.CampaignRecipients.Add(recipient);
            await context.SaveChangesAsync();

            var trackedLink = new TrackedLink 
            { 
                CampaignId = camp.Id, 
                SubscriberId = sub.Id, 
                OriginalUrl = "https://example.com/test", 
                LinkToken = linkToken 
            };
            context.TrackedLinks.Add(trackedLink);
            await context.SaveChangesAsync();

            recipientId = recipient.Id;
            trackedLinkId = trackedLink.Id;
        }

        // Act
        var client = CreateClient();
        var response = await client.GetAsync($"/api/track/click/{linkToken}");

        // Assert
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("https://example.com/test", response.Headers.Location?.ToString());

        // Verify DB update
        using (var verifyScope = _factory.Services.CreateScope())
        {
            var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedLink = await verifyDb.TrackedLinks.FirstOrDefaultAsync(t => t.Id == trackedLinkId);
            Assert.NotNull(updatedLink?.ClickedAt);

            var updatedRec = await verifyDb.CampaignRecipients.FirstOrDefaultAsync(r => r.Id == recipientId);
            Assert.NotNull(updatedRec?.ClickedAt);
        }
    }

    [Fact]
    public async Task TrackClick_InvalidToken_ShouldReturnNotFound()
    {
        var client = CreateClient();
        var response = await client.GetAsync($"/api/track/click/invalid_token_123");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
