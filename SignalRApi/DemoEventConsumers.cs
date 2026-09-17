using DemoShared;

namespace SignalRApi;

public sealed class UserTargetedSignalRConsumer
{
    public static Task Handle(UserTargetedEventMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class RoleTargetedSignalRConsumer
{
    public static Task Handle(RoleTargetedEventMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class EventTypeTargetedSignalRConsumer
{
    public static Task Handle(EventTypeTargetedEventMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}

public sealed class BroadcastSignalRConsumer
{
    public static Task Handle(BroadcastEventMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message, cancellationToken);
}
