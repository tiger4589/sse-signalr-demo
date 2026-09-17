using DemoShared;

namespace SseApi;

public sealed class UserTargetedEventMessageHandler
{
    public static Task Handle(UserTargetedEventMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class RoleTargetedEventMessageHandler
{
    public static Task Handle(RoleTargetedEventMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class EventTypeTargetedEventMessageHandler
{
    public static Task Handle(EventTypeTargetedEventMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class BroadcastEventMessageHandler
{
    public static Task Handle(BroadcastEventMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}
