namespace DemoShared;

public abstract record DemoEventMessage(DemoEvent Event);

public sealed record OrderCreatedMessage(DemoEvent Event) : DemoEventMessage(Event);
public sealed record ShipmentDelayedMessage(DemoEvent Event) : DemoEventMessage(Event);
public sealed record PaymentReceivedMessage(DemoEvent Event) : DemoEventMessage(Event);
public sealed record MaintenanceStartedMessage(DemoEvent Event) : DemoEventMessage(Event);
public sealed record SystemAlertMessage(DemoEvent Event) : DemoEventMessage(Event);
public sealed record UserNotificationMessage(DemoEvent Event) : DemoEventMessage(Event);

public static class DemoEventMessageFactory
{
    public static object Create(DemoEvent demoEvent)
    {
        ArgumentNullException.ThrowIfNull(demoEvent);

        return demoEvent.Type switch
        {
            DemoEventType.OrderCreated => new OrderCreatedMessage(demoEvent),
            DemoEventType.ShipmentDelayed => new ShipmentDelayedMessage(demoEvent),
            DemoEventType.PaymentReceived => new PaymentReceivedMessage(demoEvent),
            DemoEventType.MaintenanceStarted => new MaintenanceStartedMessage(demoEvent),
            DemoEventType.SystemAlert => new SystemAlertMessage(demoEvent),
            DemoEventType.UserNotification => new UserNotificationMessage(demoEvent),
            _ => new OrderCreatedMessage(demoEvent)
        };
    }
}
