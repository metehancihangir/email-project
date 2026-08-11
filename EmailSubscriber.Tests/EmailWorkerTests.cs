using EmailSubscriber.API.Queue;
using EmailSubscriber.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmailSubscriber.Tests;

public class EmailWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_WhenJobInQueue_SendsEmailSuccessfully() // 2.4.1
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(sp => mockEmailService.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var mockLogger = new Mock<ILogger<EmailWorker>>();
        var queueService = new EmailQueueService();

        var job = new EmailJob("test@example.com", "Test", "Subject", "Body");
        queueService.Enqueue(job);

        var worker = new EmailWorker(queueService, serviceProvider, mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        var executeTask = worker.StartAsync(cts.Token);
        
        // Allow some time for background processing
        await Task.Delay(100); 
        cts.Cancel(); // Stop listening
        await Task.WhenAny(executeTask, Task.Delay(1000));

        // Assert
        mockEmailService.Verify(s => s.SendAsync(
            "test@example.com", "Test", "Subject", "Body"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailFails_RetriesUpTo3Times() // 2.4.2 & 2.4.3
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService
            .Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP Error"));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(sp => mockEmailService.Object);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var mockLogger = new Mock<ILogger<EmailWorker>>();
        var queueService = new EmailQueueService();

        var job = new EmailJob("fail@example.com", "Fail", "Subject", "Body");
        queueService.Enqueue(job);

        var worker = new EmailWorker(queueService, serviceProvider, mockLogger.Object);
        var cts = new CancellationTokenSource();

        // Act
        var executeTask = worker.StartAsync(cts.Token);
        
        // The delays are 2s + 4s = 6s total before failing on 3rd attempt.
        // Wait long enough for 3 attempts (approx 7 seconds).
        await Task.Delay(6500); 
        cts.Cancel(); 
        await Task.WhenAny(executeTask, Task.Delay(1000));

        // Assert
        mockEmailService.Verify(s => s.SendAsync(
            "fail@example.com", "Fail", "Subject", "Body"), Times.Exactly(3));
    }
}
