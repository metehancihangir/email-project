using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace EmailSubscriber.API.Queue;

public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();

    public void Enqueue(EmailJob job)
    {
        _channel.Writer.TryWrite(job);
    }

    public async IAsyncEnumerable<EmailJob> DequeueAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(ct))
        {
            yield return job;
        }
    }
}
