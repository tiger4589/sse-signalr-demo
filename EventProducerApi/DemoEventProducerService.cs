using DemoShared;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace EventProducerApi;

public sealed class DemoEventProducerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DemoEventProducerService> _logger;

    public DemoEventProducerService(IServiceScopeFactory scopeFactory, ILogger<DemoEventProducerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(7));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var demoEvent = DemoEventFactory.CreateRandomEvent();
            var message = DemoEventMessageFactory.Create(demoEvent);
            await messageBus.PublishAsync(message);
            _logger.LogInformation("[Producer] Random event: {EventType} -> {Target}", demoEvent.Type, demoEvent.TargetLabel);
        }
    }
}
