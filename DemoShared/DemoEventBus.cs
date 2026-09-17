namespace DemoShared;

public abstract record DemoEventMessage(DemoEvent Event);

public sealed record UserTargetedEventMessage(DemoEvent Event, string UserId) : DemoEventMessage(Event);
public sealed record RoleTargetedEventMessage(DemoEvent Event, string Role) : DemoEventMessage(Event);
public sealed record EventTypeTargetedEventMessage(DemoEvent Event, string EventType) : DemoEventMessage(Event);
public sealed record BroadcastEventMessage(DemoEvent Event) : DemoEventMessage(Event);

public static class DemoEventMessageFactory
{
    public static object Create(DemoEvent demoEvent)
    {
        ArgumentNullException.ThrowIfNull(demoEvent);

        return demoEvent.Target.Kind switch
        {
            DemoTargetKind.User => new UserTargetedEventMessage(demoEvent, RequireTargetValue(demoEvent.Target, DemoTargetKind.User)),
            DemoTargetKind.Role => new RoleTargetedEventMessage(demoEvent, RequireTargetValue(demoEvent.Target, DemoTargetKind.Role)),
            DemoTargetKind.EventType => new EventTypeTargetedEventMessage(demoEvent, RequireTargetValue(demoEvent.Target, DemoTargetKind.EventType)),
            DemoTargetKind.Broadcast => new BroadcastEventMessage(demoEvent),
            _ => throw new InvalidOperationException($"Unsupported target kind '{demoEvent.Target.Kind}'.")
        };
    }

    private static string RequireTargetValue(DemoEventTarget target, DemoTargetKind expectedKind)
    {
        if (target.Kind != expectedKind)
        {
            throw new InvalidOperationException($"Expected target kind '{expectedKind}' but got '{target.Kind}'.");
        }

        if (string.IsNullOrWhiteSpace(target.Value))
        {
            throw new InvalidOperationException($"Target value is required for '{expectedKind}'.");
        }

        return target.Value;
    }
}
