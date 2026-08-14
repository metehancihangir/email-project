using System.Net;
using System.Net.Http.Headers;
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

public class NewsletterIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public NewsletterIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:Default", "Server=localhost;Database=dummy;Uid=root;Pwd=;" },
                    { "Admin:Username", "admin" },
                    { "Admin:PasswordHash", "$2a$11$iGiG4IPNiEuEqD1t9Lfmve/NNf2j9mndB3IyqjkSBKixqxYHEfgtC" }
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
                    options.UseInMemoryDatabase("InMemoryDbForNewsletterTesting");
                });
            });
        });
    }

    private async Task<string> GetValidTokenAsync(HttpClient client)
    {
        var request = new LoginRequest("admin", "password123");
        var response = await client.PostAsJsonAsync("/api/admin/login", request);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return content!["token"];
    }

    [Fact]
    public async Task Post_Newsletter_EmptySubject_ReturnsBadRequest() // 5.4.4 Backend side test for validation
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new NewsletterSendRequest { Subject = "", HtmlBody = "<p>Test</p>" };
        var response = await client.PostAsJsonAsync("/api/admin/newsletter", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_Newsletter_ValidData_CreatesCampaignAndIgnoresInactive() // 5.4.1 & 5.4.2
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Add subscribers
            db.Subscribers.Add(new Subscriber { Email = "active1@test.com", IsActive = true, IsConfirmed = true });
            db.Subscribers.Add(new Subscriber { Email = "active2@test.com", IsActive = true, IsConfirmed = true });
            db.Subscribers.Add(new Subscriber { Email = "inactive@test.com", IsActive = false, IsConfirmed = true }); // Should ignore
            db.Subscribers.Add(new Subscriber { Email = "unconfirmed@test.com", IsActive = true, IsConfirmed = false }); // Should ignore
            await db.SaveChangesAsync();
        }

        var request = new NewsletterSendRequest { Subject = "Test Bülten", HtmlBody = "<p>Hello World</p>" };
        var response = await client.PostAsJsonAsync("/api/admin/newsletter", request);
        
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        
        Assert.True(result!.ContainsKey("campaignId"));
        Assert.Equal(2, result["recipientCount"]); // Only active1 and active2

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var campaign = await db.Campaigns.Include(c => c.Recipients).FirstOrDefaultAsync(c => c.Id == result["campaignId"]);
            
            Assert.NotNull(campaign);
            Assert.Equal("Test Bülten", campaign.Subject);
            Assert.Equal(2, campaign.RecipientCount);
            Assert.Equal(2, campaign.Recipients.Count);
        }
    }
}
