using System.Net;
using System.Net.Http.Json;
using EmailSubscriber.API.Data;
using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmailSubscriber.Tests;

public class SubscribersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SubscribersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        // In-memory database kullanarak WebApplicationFactory oluştur.
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
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
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    [Fact]
    public async Task Post_ValidEmail_ReturnsAcceptedAndSavesToDb() // 1.4.1
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SubscribeRequest("test@example.com", "Test User", null);

        // Act
        var response = await client.PostAsJsonAsync("/api/subscribers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var subscriber = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == "test@example.com");
        
        Assert.NotNull(subscriber);
        Assert.Equal("Test User", subscriber.Name);
        Assert.False(subscriber.IsConfirmed);
    }

    [Fact]
    public async Task Post_InvalidEmail_ReturnsBadRequest() // 1.4.2
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SubscribeRequest("invalid-email", "Test User", null);

        // Act
        var response = await client.PostAsJsonAsync("/api/subscribers", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_DuplicateConfirmedEmail_ReturnsConflict() // 1.4.3
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SubscribeRequest("duplicate@example.com", "User", null);

        // Seed DB with confirmed subscriber
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (!db.Subscribers.Any(s => s.Email == "duplicate@example.com"))
            {
                db.Subscribers.Add(new Subscriber
                {
                    Email = "duplicate@example.com",
                    IsConfirmed = true
                });
                await db.SaveChangesAsync();
            }
        }

        // Act
        var response = await client.PostAsJsonAsync("/api/subscribers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_HoneypotFilled_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SubscribeRequest("bot@example.com", "Bot", "http://spam.com"); // website is filled

        // Act
        var response = await client.PostAsJsonAsync("/api/subscribers", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_RateLimiting_ReturnsTooManyRequests() // 1.4.4
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SubscribeRequest("rate@example.com", "Rate User", null);

        // Act - Send 101 requests rapidly. Limit is 100 per minute.
        HttpResponseMessage lastResponse = null;
        for (int i = 0; i < 101; i++)
        {
            lastResponse = await client.PostAsJsonAsync("/api/subscribers", request);
        }

        // Assert
        // The 101st request should be 429 Too Many Requests
        Assert.NotNull(lastResponse);
        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Confirm_ValidToken_ReturnsOk() // 3.3.1
    {
        // Arrange
        var client = _factory.CreateClient();
        string token = Guid.NewGuid().ToString("N");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = "confirm@example.com", ConfirmationToken = token, ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24) });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/subscribers/confirm/{token}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sub = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == "confirm@example.com");
            Assert.True(sub.IsConfirmed);
            // We nullify the token after confirmation.
            Assert.Null(sub.ConfirmationToken);
        }
    }

    [Fact]
    public async Task Get_Confirm_InvalidToken_ReturnsNotFound() // 3.3.3
    {
        // Arrange
        var client = _factory.CreateClient();
        string token = "invalid-token";

        // Act
        var response = await client.GetAsync($"/api/subscribers/confirm/{token}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Confirm_AlreadyConfirmed_ReturnsOkIdempotent() // 3.3.4
    {
        // Arrange
        var client = _factory.CreateClient();
        string token = Guid.NewGuid().ToString("N");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = "idempotent@example.com", ConfirmationToken = token, IsConfirmed = true });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/subscribers/confirm/{token}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Confirm_ExpiredToken_ReturnsGone() // 3.3.2
    {
        // Arrange
        var client = _factory.CreateClient();
        string token = Guid.NewGuid().ToString("N");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = "expired@example.com", ConfirmationToken = token, ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(-1) });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/subscribers/confirm/{token}");

        // Assert
        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact]
    public async Task Post_ResendConfirmation_ValidEmail_ReturnsAccepted()
    {
        // Arrange
        var client = _factory.CreateClient();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = "resend@example.com", IsConfirmed = false });
            await db.SaveChangesAsync();
        }

        // Act
        var request = new ResendRequest("resend@example.com");
        var response = await client.PostAsJsonAsync("/api/subscribers/resend-confirmation", request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    public async Task Get_Unsubscribe_ValidToken_ReturnsOk() // 3.3.5
    {
        // Arrange
        var client = _factory.CreateClient();
        string email = "unsubscribe@example.com";
        string token = "test-unsub-token-123";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = email, IsActive = true, UnsubscribeToken = token });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync($"/api/subscribers/unsubscribe/{token}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sub = await db.Subscribers.FirstOrDefaultAsync(s => s.Email == email);
            Assert.False(sub.IsActive);
            Assert.NotNull(sub.UnsubscribedAt);
        }
    }
}
