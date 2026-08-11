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
using System.Net.Http.Headers;

namespace EmailSubscriber.Tests;

public class AdminIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdminIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
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
                    options.UseInMemoryDatabase("InMemoryDbForAdminTesting");
                });
            });
        });
    }

    [Fact]
    public async Task Post_Login_InvalidCredentials_ReturnsUnauthorized() // 4.4.1
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("admin", "wrongpassword");
        
        var response = await client.PostAsJsonAsync("/api/admin/login", request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_Login_ValidCredentials_ReturnsOkWithToken() // 4.4.2
    {
        var client = _factory.CreateClient();
        // Varsayılan appsettings içindeki şifre password123
        var request = new LoginRequest("admin", "password123");
        
        var response = await client.PostAsJsonAsync("/api/admin/login", request);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", content);
    }

    [Fact]
    public async Task Get_Subscribers_WithoutToken_ReturnsUnauthorized() // 4.4.3
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/admin/subscribers");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> GetValidTokenAsync(HttpClient client)
    {
        var request = new LoginRequest("admin", "password123");
        var response = await client.PostAsJsonAsync("/api/admin/login", request);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return content!["token"];
    }

    [Fact]
    public async Task Get_Subscribers_WithToken_AndFilter_ReturnsFilteredList() // 4.4.4
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Subscribers.Add(new Subscriber { Email = "active@test.com", IsActive = true, IsConfirmed = true });
            db.Subscribers.Add(new Subscriber { Email = "inactive@test.com", IsActive = false, IsConfirmed = true });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/admin/subscribers?isActive=true");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("active@test.com", content);
        Assert.DoesNotContain("inactive@test.com", content);
    }

    [Fact]
    public async Task Put_DeactivateSubscriber_UpdatesIsActiveToFalse() // 4.4.5
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        int subId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sub = new Subscriber { Email = "todeactivate@test.com", IsActive = true, IsConfirmed = true };
            db.Subscribers.Add(sub);
            await db.SaveChangesAsync();
            subId = sub.Id;
        }

        var response = await client.PutAsync($"/api/admin/subscribers/{subId}/deactivate", null);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedSub = await db.Subscribers.FindAsync(subId);
            Assert.False(updatedSub!.IsActive);
        }
    }

    [Fact]
    public async Task Get_Stats_ReturnsCorrectCounts() // 4.4.6
    {
        var client = _factory.CreateClient();
        string token = await GetValidTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Add a few subscribers to ensure stats have numbers
            db.Subscribers.Add(new Subscriber { Email = "stat1@test.com", IsActive = true, IsConfirmed = true, SubscribedAt = DateTime.UtcNow });
            db.Subscribers.Add(new Subscriber { Email = "stat2@test.com", IsActive = true, IsConfirmed = false, SubscribedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync("/api/admin/stats");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("total", content);
        Assert.Contains("active", content);
        Assert.Contains("unconfirmed", content);
        Assert.Contains("today", content);
    }
}
