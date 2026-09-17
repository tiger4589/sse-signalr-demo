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

    public Task DispatchAsync(UserTargetedEventMessage message, CancellationToken cancellationToken) =>
        DispatchToTargetsAsync(
            message.Event,
            _registry.GetConnectionsForUser(message.UserId),
            $"user:{message.UserId}",
            cancellationToken);

    public Task DispatchAsync(RoleTargetedEventMessage message, CancellationToken cancellationToken) =>
        DispatchToTargetsAsync(
            message.Event,
            _registry.GetConnectionsForRole(message.Role),
            $"role:{message.Role}",
            cancellationToken);

    public Task DispatchAsync(EventTypeTargetedEventMessage message, CancellationToken cancellationToken) =>
        DispatchToTargetsAsync(
            message.Event,
            _registry.GetConnectionsForEventType(message.EventType),
            $"event-type:{message.EventType}",
            cancellationToken);

    public Task DispatchAsync(BroadcastEventMessage message, CancellationToken cancellationToken) =>
        DispatchToTargetsAsync(
            message.Event,
            _registry.GetAllConnections(),
            "broadcast",
            cancellationToken);

    private Task DispatchToTargetsAsync(
        DemoEvent demoEvent,
        IEnumerable<SseConnectionState> targetConnections,
        string targetDescription,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targets = targetConnections.ToList();
        _logger.LogInformation("[SSE] Sending {EventType} to {Target} ({Count} connection(s))", demoEvent.Type, targetDescription, targets.Count);

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
