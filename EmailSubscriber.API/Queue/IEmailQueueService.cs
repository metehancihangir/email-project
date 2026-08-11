namespace EmailSubscriber.API.Queue;

public interface IEmailQueueService
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken ct);
}
