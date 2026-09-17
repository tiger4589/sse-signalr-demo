using DemoShared;

namespace SseApi;

public sealed class OrderCreatedSseConsumer
{
    public static Task Handle(OrderCreatedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class ShipmentDelayedSseConsumer
{
    public static Task Handle(ShipmentDelayedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class PaymentReceivedSseConsumer
{
    public static Task Handle(PaymentReceivedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class MaintenanceStartedSseConsumer
{
    public static Task Handle(MaintenanceStartedMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class SystemAlertSseConsumer
{
    public static Task Handle(SystemAlertMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}

public sealed class UserNotificationSseConsumer
{
    public static Task Handle(UserNotificationMessage message, SseMessageDispatcher dispatcher, CancellationToken cancellationToken)
        => dispatcher.DispatchAsync(message.Event, cancellationToken);
}
