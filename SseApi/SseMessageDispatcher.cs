using DemoShared;

namespace SseApi;

public sealed class SseMessageDispatcher
{
    private readonly SseConnectionRegistry _registry;
    private readonly ILogger<SseMessageDispatcher> _logger;

    public SseMessageDispatcher(SseConnectionRegistry registry, ILogger<SseMessageDispatcher> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public Task DispatchAsync(DemoEvent demoEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targets = _registry.GetTargets(demoEvent).Distinct().ToList();
        _logger.LogInformation("[SSE] Evaluating {EventType} for {Count} connection(s)", demoEvent.Type, targets.Count);

        foreach (var connection in targets)
        {
            if (!connection.TryQueueEvent(demoEvent))
            {
                _logger.LogWarning("[SSE] Unable to queue event for {UserId}", connection.UserId);
                continue;
            }

            _logger.LogInformation("[SSE] Sending {EventType} to {UserId}", demoEvent.Type, connection.UserId);
        }

        return Task.CompletedTask;
    }
}
