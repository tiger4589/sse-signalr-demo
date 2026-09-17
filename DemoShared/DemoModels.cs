using System.Collections.Concurrent;

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
    Random
}

public sealed record DemoUser(
    string Id,
    string Name,
    string Role);

public sealed record DemoEvent(
    Guid Id,
    DemoEventType Type,
    string? UserId,
    string? TargetRole,
    DateTimeOffset Timestamp,
    string Message,
    bool BroadcastToEveryone = false)
{
    public string TargetLabel =>
        BroadcastToEveryone ? "Everyone" :
        !string.IsNullOrWhiteSpace(UserId) ? $"User:{UserId}" :
        !string.IsNullOrWhiteSpace(TargetRole) ? $"Role:{TargetRole}" :
        "All";
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

    public static DemoEvent CreateRandomEvent()
    {
        var type = Enum.GetValues<DemoEventType>()[Random.Next(Enum.GetValues<DemoEventType>().Length)];
        var user = DemoUserCatalog.Users[Random.Next(DemoUserCatalog.Users.Count)];

        return type switch
        {
            DemoEventType.OrderCreated => new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, null, null, DateTimeOffset.Now, "Order created for all users.", false),
            DemoEventType.ShipmentDelayed => new DemoEvent(Guid.NewGuid(), DemoEventType.ShipmentDelayed, null, "Operator", DateTimeOffset.Now, "Shipment delayed for operators.", false),
            DemoEventType.PaymentReceived => new DemoEvent(Guid.NewGuid(), DemoEventType.PaymentReceived, user.Id, null, DateTimeOffset.Now, $"Payment received for {user.Name}.", false),
            DemoEventType.MaintenanceStarted => new DemoEvent(Guid.NewGuid(), DemoEventType.MaintenanceStarted, null, null, DateTimeOffset.Now, "Maintenance started for all users.", false),
            DemoEventType.SystemAlert => new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, null, "Finance", DateTimeOffset.Now, "System alert for finance.", false),
            DemoEventType.UserNotification => new DemoEvent(Guid.NewGuid(), DemoEventType.UserNotification, user.Id, null, DateTimeOffset.Now, $"Personal notification for {user.Name}.", false),
            _ => new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, null, null, DateTimeOffset.Now, "Random demo event.", false)
        };
    }

    public static IReadOnlyList<DemoEvent> BuildScenario(DemoScenario scenario)
    {
        return scenario switch
        {
            DemoScenario.NormalOperations =>
            [
                new(Guid.NewGuid(), DemoEventType.OrderCreated, null, null, DateTimeOffset.Now, "Order created for all users."),
                new(Guid.NewGuid(), DemoEventType.OrderCreated, null, null, DateTimeOffset.Now, "Second order created for all users."),
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, null, "Operator", DateTimeOffset.Now, "Shipment delayed for operators."),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, "Alice", null, DateTimeOffset.Now, "Payment received for Alice.")
            ],
            DemoScenario.OperationsIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, null, "Operator", DateTimeOffset.Now, "Shipment delayed for operators."),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, "Operator", DateTimeOffset.Now, "System alert for operators."),
                new(Guid.NewGuid(), DemoEventType.MaintenanceStarted, null, null, DateTimeOffset.Now, "Maintenance work started.")
            ],
            DemoScenario.PaymentIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, "Alice", null, DateTimeOffset.Now, "Payment received for Alice."),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, "Bob", null, DateTimeOffset.Now, "Payment received for Bob."),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, "Finance", DateTimeOffset.Now, "Finance alert: payment issue detected.")
            ],
            DemoScenario.UserNotification =>
            [
                new(Guid.NewGuid(), DemoEventType.UserNotification, "Alice", null, DateTimeOffset.Now, "Alice received a user notification.")
            ],
            DemoScenario.AlertFinance =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, "Finance", DateTimeOffset.Now, "Finance alert issued.")
            ],
            DemoScenario.BroadcastEmergency =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, null, DateTimeOffset.Now, "Emergency broadcast to everyone.", true)
            ],
            _ => [CreateRandomEvent()]
        };
    }
}
