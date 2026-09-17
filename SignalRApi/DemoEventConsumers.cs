using DemoShared;

namespace SignalRApi;

public sealed class OrderCreatedSignalRConsumer
{
    public static Task Handle(OrderCreatedMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class ShipmentDelayedSignalRConsumer
{
    public static Task Handle(ShipmentDelayedMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class PaymentReceivedSignalRConsumer
{
    public static Task Handle(PaymentReceivedMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class MaintenanceStartedSignalRConsumer
{
    public static Task Handle(MaintenanceStartedMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class SystemAlertSignalRConsumer
{
    public static Task Handle(SystemAlertMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class UserNotificationSignalRConsumer
{
    public static Task Handle(UserNotificationMessage message, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}
