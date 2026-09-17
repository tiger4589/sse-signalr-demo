using DemoShared;
using Wolverine;

namespace EventProducerApi;

public sealed class EventFanOutPublisher
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<EventFanOutPublisher> _logger;

    public EventFanOutPublisher(IMessageBus messageBus, ILogger<EventFanOutPublisher> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task PublishAsync(DemoEvent demoEvent, CancellationToken cancellationToken)
    {
        var message = DemoEventMessageFactory.Create(demoEvent);
        await _messageBus.PublishAsync(message);
        _logger.LogInformation("[Producer] Published {EventType} through Wolverine", demoEvent.Type);
    }
}
