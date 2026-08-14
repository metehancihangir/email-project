using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace EmailSubscriber.API.Queue;

public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailJob> _channel = Channel.CreateBounded<EmailJob>(new BoundedChannelOptions(10000)
    {
        FullMode = BoundedChannelFullMode.Wait
    });

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
