namespace DemoShared;

public enum DemoEventType
{
    OrderCreated,
    ShipmentDelayed,
    PaymentReceived,
    MaintenanceStarted,
    SystemAlert,
    UserNotification
}

public enum DemoScenario
{
    NormalOperations,
    OperationsIncident,
    PaymentIncident,
    UserNotification,
    AlertFinance,
    BroadcastEmergency,
    Orders,
    Shipments,
    Payments,
    Maintenance,
    SystemAlerts,
    NotifyCharlie,
    Random
}

public sealed record DemoUser(
    string Id,
    string Name,
    string Role);

public enum DemoTargetKind
{
    User,
    Role,
    EventType,
    Broadcast
}

public sealed record DemoEventTarget(DemoTargetKind Kind, string? Value = null)
{
    public static DemoEventTarget User(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return new DemoEventTarget(DemoTargetKind.User, userId);
    }

    public static DemoEventTarget Role(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return new DemoEventTarget(DemoTargetKind.Role, role);
    }

    public static DemoEventTarget EventType(string eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        return new DemoEventTarget(DemoTargetKind.EventType, eventType);
    }

    public static DemoEventTarget EventType(DemoEventType eventType) =>
        EventType(eventType.ToFriendlyName());

    public static DemoEventTarget Broadcast() => new(DemoTargetKind.Broadcast);

    public string Label => Kind switch
    {
        DemoTargetKind.User => $"User:{Value}",
        DemoTargetKind.Role => $"Role:{Value}",
        DemoTargetKind.EventType => $"EventType:{Value}",
        DemoTargetKind.Broadcast => "Everyone",
        _ => "Unknown"
    };
}

public sealed record DemoEvent(
    Guid Id,
    DemoEventType Type,
    DateTimeOffset Timestamp,
    string Message,
    DemoEventTarget Target)
{
    public string TargetLabel => Target.Label;
}

public static class DemoUserCatalog
{
    public static readonly IReadOnlyList<DemoUser> Users =
    [
        new("Alice", "Alice", "Operator"),
        new("Bob", "Bob", "Operator"),
        new("Charlie", "Charlie", "Manager"),
        new("Diana", "Diana", "Finance"),
        new("Admin", "Admin", "Administrator")
    ];

    public static DemoUser? Get(string? userId) =>
        string.IsNullOrWhiteSpace(userId) ? null : Users.FirstOrDefault(u => string.Equals(u.Id, userId, StringComparison.OrdinalIgnoreCase));
}

public static class DemoEventTypeExtensions
{
    public static string ToFriendlyName(this DemoEventType type) => type switch
    {
        DemoEventType.OrderCreated => "Orders",
        DemoEventType.ShipmentDelayed => "Shipments",
        DemoEventType.PaymentReceived => "Payments",
        DemoEventType.MaintenanceStarted => "Maintenance",
        DemoEventType.SystemAlert => "System Alerts",
        DemoEventType.UserNotification => "User Notifications",
        _ => type.ToString()
    };

    public static string ToFriendlyEventTypeName(this string eventTypeName)
    {
        if (Enum.TryParse<DemoEventType>(eventTypeName, true, out var parsed))
        {
            return parsed.ToFriendlyName();
        }

        return eventTypeName;
    }
}

public static class DemoEventFactory
{
    private static readonly Random Random = new();
    private static readonly string[] RoleTargets = ["Operator", "Manager"];

    public static DemoEvent CreateRandomEvent()
    {
        var type = Enum.GetValues<DemoEventType>()[Random.Next(Enum.GetValues<DemoEventType>().Length)];
        var user = DemoUserCatalog.Users[Random.Next(DemoUserCatalog.Users.Count)];
        var role = RoleTargets[Random.Next(RoleTargets.Length)];
        var strategy = Random.Next(4);

        return strategy switch
        {
            0 => new DemoEvent(Guid.NewGuid(), type, DateTimeOffset.Now, $"User-targeted {type} event for {user.Name}.", DemoEventTarget.User(user.Id)),
            1 => new DemoEvent(Guid.NewGuid(), type, DateTimeOffset.Now, $"Role-targeted {type} event for {role}.", DemoEventTarget.Role(role)),
            2 => new DemoEvent(Guid.NewGuid(), type, DateTimeOffset.Now, $"Event-type targeted {type} event for subscribers.", DemoEventTarget.EventType(type)),
            _ => new DemoEvent(Guid.NewGuid(), type, DateTimeOffset.Now, $"Broadcast {type} event to everyone.", DemoEventTarget.Broadcast())
        };
    }

    public static IReadOnlyList<DemoEvent> BuildScenario(DemoScenario scenario)
    {
        return scenario switch
        {
            DemoScenario.Orders =>
            [
                new(Guid.NewGuid(), DemoEventType.OrderCreated, DateTimeOffset.Now, "Order created event for order subscribers.", DemoEventTarget.EventType(DemoEventType.OrderCreated))
            ],
            DemoScenario.Shipments =>
            [
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, DateTimeOffset.Now, "Shipment delayed event for shipment subscribers.", DemoEventTarget.EventType(DemoEventType.ShipmentDelayed))
            ],
            DemoScenario.Payments =>
            [
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.Now, "Payment received event for payment subscribers.", DemoEventTarget.EventType(DemoEventType.PaymentReceived))
            ],
            DemoScenario.Maintenance =>
            [
                new(Guid.NewGuid(), DemoEventType.MaintenanceStarted, DateTimeOffset.Now, "Maintenance started event for maintenance subscribers.", DemoEventTarget.EventType(DemoEventType.MaintenanceStarted))
            ],
            DemoScenario.SystemAlerts =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.Now, "System alert event for system alert subscribers.", DemoEventTarget.EventType(DemoEventType.SystemAlert))
            ],
            DemoScenario.NormalOperations =>
            [
                new(Guid.NewGuid(), DemoEventType.OrderCreated, DateTimeOffset.Now, "Order created notification for order subscribers.", DemoEventTarget.EventType(DemoEventType.OrderCreated)),
                new(Guid.NewGuid(), DemoEventType.OrderCreated, DateTimeOffset.Now, "Second order update for order subscribers.", DemoEventTarget.EventType(DemoEventType.OrderCreated)),
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, DateTimeOffset.Now, "Shipment delayed for operators.", DemoEventTarget.Role("Operator")),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.Now, "Payment received for Alice.", DemoEventTarget.User("Alice"))
            ],
            DemoScenario.OperationsIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, DateTimeOffset.Now, "Shipment delayed for operators.", DemoEventTarget.Role("Operator")),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.Now, "System alert for operators.", DemoEventTarget.Role("Operator")),
                new(Guid.NewGuid(), DemoEventType.MaintenanceStarted, DateTimeOffset.Now, "Maintenance started for maintenance subscribers.", DemoEventTarget.EventType(DemoEventType.MaintenanceStarted))
            ],
            DemoScenario.PaymentIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.Now, "Payment received for Alice.", DemoEventTarget.User("Alice")),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.Now, "Payment received for Bob.", DemoEventTarget.User("Bob")),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.Now, "Finance alert: payment issue detected.", DemoEventTarget.Role("Finance"))
            ],
            DemoScenario.UserNotification =>
            [
                new(Guid.NewGuid(), DemoEventType.UserNotification, DateTimeOffset.Now, "Alice received a user notification.", DemoEventTarget.User("Alice"))
            ],
            DemoScenario.NotifyCharlie =>
            [
                new(Guid.NewGuid(), DemoEventType.UserNotification, DateTimeOffset.Now, "Charlie received a user notification.", DemoEventTarget.User("Charlie"))
            ],
            DemoScenario.AlertFinance =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.Now, "Finance alert issued.", DemoEventTarget.Role("Finance"))
            ],
            DemoScenario.BroadcastEmergency =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.Now, "Emergency broadcast to everyone.", DemoEventTarget.Broadcast())
            ],
            _ => [CreateRandomEvent()]
        };
    }
}
