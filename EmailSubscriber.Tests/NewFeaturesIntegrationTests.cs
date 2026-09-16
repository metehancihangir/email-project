using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmailSubscriber.API.Data;
using EmailSubscriber.API.DTOs;
using EmailSubscriber.API.Models;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmailSubscriber.Tests;

public class NewFeaturesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public NewFeaturesIntegrationTests(WebApplicationFactory<Program> factory)
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
                    { "ConnectionStrings:Default", "Server=localhost;Database=dummy;Uid=root;Pwd=;" },
                    { "Admin:Username", "admin" },
                    { "Admin:PasswordHash", TestConfiguration.AdminPasswordHash }
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
                    options.UseInMemoryDatabase("InMemoryDbForNewFeaturesTesting");
                });
            });
        });
    }

    private async Task<string> GetValidTokenAsync(HttpClient client)
    {
        var request = new LoginRequest("admin", TestConfiguration.AdminPassword);
        var response = await client.PostAsJsonAsync("/api/admin/login", request);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return content!["token"];
    }

    [Fact]
    public async Task Get_PublicArchive_ReturnsArchivedNewsletters()
    {
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Campaigns.Add(new Campaign
            {
                Subject = "Mitoloji Bülteni: Zeus Efsanesi",
                HtmlBody = "<p>Zeus antik Yunan mitolojisinin baş tanrısıdır.</p>",
                Category = "Mitoloji",
                CoverImageUrl = "https://images.unsplash.com/zeus.jpg",
                RecipientCount = 10
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/subscribers/archive?category=Mitoloji");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PublicNewsletterDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.Contains(result.Items, i => i.Subject.Contains("Zeus"));
    }

    [Fact]
    public async Task Track_Feedback_ValidSignature_RecordsVote()
    {
        var client = _factory.CreateClient();
        int campaignId;
        int subscriberId;
        string sig;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tracking = scope.ServiceProvider.GetRequiredService<ITrackingService>();

            var sub = new Subscriber { Email = "feedback_voter@test.com", IsActive = true, IsConfirmed = true };
            db.Subscribers.Add(sub);

            var camp = new Campaign { Subject = "Geri Bildirim Testi", HtmlBody = "<p>Test</p>" };
            db.Campaigns.Add(camp);
            await db.SaveChangesAsync();

            campaignId = camp.Id;
            subscriberId = sub.Id;
            sig = tracking.GenerateOpenSignature(campaignId, subscriberId);
        }

        var response = await client.GetAsync($"/api/track/feedback/{campaignId}/{subscriberId}?isPositive=true&sig={sig}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Geri Bildiriminiz Alındı", html);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var vote = await db.CampaignFeedbacks.FirstOrDefaultAsync(f => f.CampaignId == campaignId && f.SubscriberId == subscriberId);
            Assert.NotNull(vote);
            Assert.True(vote.IsPositive);
        }
    }

    [Fact]
    public async Task Post_SendTestEmail_QueuesTestMessage()
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new SendTestEmailRequest
        {
            TargetEmail = "admin_test@test.com",
            Subject = "Test E-postası Önizlemesi",
            HtmlBody = "<p>Bu bir test içeriğidir.</p>",
            Category = "Finans"
        };

        var response = await client.PostAsJsonAsync("/api/admin/send-test", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
