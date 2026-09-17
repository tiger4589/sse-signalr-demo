using DemoShared;

namespace SseApi;

public sealed class OrderCreatedMessageHandler
{
    public static Task Handle(OrderCreatedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class ShipmentDelayedMessageHandler
{
    public static Task Handle(ShipmentDelayedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class PaymentReceivedMessageHandler
{
    public static Task Handle(PaymentReceivedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class MaintenanceStartedMessageHandler
{
    public static Task Handle(MaintenanceStartedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class SystemAlertMessageHandler
{
    public static Task Handle(SystemAlertMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class UserNotificationMessageHandler
{
    public static Task Handle(UserNotificationMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}
