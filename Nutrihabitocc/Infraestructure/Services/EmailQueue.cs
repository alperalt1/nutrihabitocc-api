using System.Threading.Channels;
using Nutrihabitocc.Domain.Dtos;

namespace Nutrihabitocc.Infraestructure.Services;

public class EmailQueue
{
    private readonly Channel<EmailMessage> _queue;

    public EmailQueue()
    {
        _queue = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
        {
            SingleReader = true
        });
    }

    public ValueTask QueueEmailAsync(EmailMessage message)
    {
        return _queue.Writer.WriteAsync(message);
    }

    public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}