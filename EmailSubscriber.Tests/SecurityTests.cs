using System.Net;
using EmailSubscriber.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EmailSubscriber.Tests;

public class SecurityTests
{
    [Fact]
    public async Task EmailTemplateService_HtmlEncodesMaliciousName_PreventsHtmlInjection()
    {
        // Arrange
        var mockEnv = new Mock<IWebHostEnvironment>();
        
        // Use directory of the API project
        var apiPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "EmailSubscriber.API"));
        mockEnv.Setup(e => e.ContentRootPath).Returns(apiPath);

        var service = new EmailTemplateService(mockEnv.Object);
        var maliciousName = "<script>alert('xss')</script><a href='https://phishing.com'>Click</a>";

        // Act
        var confirmHtml = await service.GetConfirmationEmailHtmlAsync(maliciousName, "https://example.com/confirm");
        var welcomeHtml = await service.GetWelcomeEmailHtmlAsync(maliciousName, "https://example.com/unsub", "https://example.com/pref");

        // Assert
        Assert.DoesNotContain("<script>", confirmHtml);
        Assert.DoesNotContain("<a href='https://phishing.com'>", confirmHtml);
        Assert.Contains(WebUtility.HtmlEncode(maliciousName), confirmHtml);

        Assert.DoesNotContain("<script>", welcomeHtml);
        Assert.DoesNotContain("<a href='https://phishing.com'>", welcomeHtml);
        Assert.Contains(WebUtility.HtmlEncode(maliciousName), welcomeHtml);
    }

    [Fact]
    public void TrackingService_ValidatesHmacSignature_RejectsTamperedSignatures()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Secret", "TestSecretKeyForHmacValidation2026SecureKey!"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var trackingService = new TrackingService(null!, configuration);

        // Act
        var validSig = trackingService.GenerateOpenSignature(1, 42);

        // Assert
        Assert.NotEmpty(validSig);
        Assert.True(trackingService.ValidateOpenSignature(1, 42, validSig));
        Assert.False(trackingService.ValidateOpenSignature(1, 42, "invalid_sig_1234"));
        Assert.False(trackingService.ValidateOpenSignature(1, 42, null));
        Assert.False(trackingService.ValidateOpenSignature(1, 43, validSig)); // Different subscriber
        Assert.False(trackingService.ValidateOpenSignature(2, 42, validSig)); // Different campaign
    }

    [Theory]
    [InlineData("127.0.0.1", false)] // Loopback
    [InlineData("10.0.0.5", false)] // Private Network 10.x
    [InlineData("192.168.1.1", false)] // Private Network 192.168.x
    [InlineData("172.16.0.1", false)] // Private Network 172.16.x
    [InlineData("169.254.169.254", false)] // Link-Local / Cloud Metadata
    [InlineData("8.8.8.8", true)] // Public DNS
    [InlineData("93.184.216.34", true)] // Public Web IP
    public void GeminiService_IsSafeIpAddress_FiltersDangerousIps(string ipStr, bool expectedSafe)
    {
        var ip = IPAddress.Parse(ipStr);
        var isSafe = EmailSubscriber.API.Services.AI.GeminiService.IsSafeIpAddress(ip);
        Assert.Equal(expectedSafe, isSafe);
    }

    [Fact]
    public void AdminService_PasswordHashing_CorrectlyVerifiesBcryptHash()
    {
        // Arrange
        var password = "Admin12345!Secure";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act & Assert
        Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword123!", hash));
    }
}

