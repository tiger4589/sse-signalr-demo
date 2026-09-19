using System.Net.ServerSentEvents;
using System.Threading.Channels;
using DemoShared;

namespace SseApi;

public sealed class SseConnectionState
{
    public string UserId { get; set; } = string.Empty;
    public HashSet<string> Roles { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> EventTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
    private readonly Channel<SseItem<DemoEvent>> _events = Channel.CreateUnbounded<SseItem<DemoEvent>>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public bool TryQueueEvent(DemoEvent demoEvent) => _events.Writer.TryWrite(
        new SseItem<DemoEvent>(demoEvent, "message")
        {
            EventId = demoEvent.Id.ToString()
        });

    public IAsyncEnumerable<SseItem<DemoEvent>> ReadEventsAsync(CancellationToken cancellationToken = default) =>
        _events.Reader.ReadAllAsync(cancellationToken);

    public void Complete() => _events.Writer.TryComplete();
}