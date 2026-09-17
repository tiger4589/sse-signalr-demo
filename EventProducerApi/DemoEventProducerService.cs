using DemoShared;

namespace EventProducerApi;

public sealed class DemoEventProducerService : BackgroundService
{
    private readonly EventFanOutPublisher _publisher;
    private readonly ILogger<DemoEventProducerService> _logger;

    public DemoEventProducerService(EventFanOutPublisher publisher, ILogger<DemoEventProducerService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(7));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var demoEvent = DemoEventFactory.CreateRandomEvent();
            await _publisher.PublishAsync(demoEvent, stoppingToken);
            _logger.LogInformation("[Producer] Random event: {EventType} -> {Target}", demoEvent.Type, demoEvent.TargetLabel);
        }
    }
}
