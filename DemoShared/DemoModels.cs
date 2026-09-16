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
    BrusselsIncident,
    PaymentIncident,
    UserNotification,
    AlertFinance,
    BroadcastEmergency,
    Random
}

public sealed record DemoUser(
    string Id,
    string Name,
    string Role,
    string Warehouse);

public sealed record DemoEvent(
    Guid Id,
    DemoEventType Type,
    string? Warehouse,
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
        !string.IsNullOrWhiteSpace(Warehouse) ? $"Warehouse:{Warehouse}" : "All";
}

public static class DemoUserCatalog
{
    public static readonly IReadOnlyList<DemoUser> Users =
    [
        new("Alice", "Alice", "Operator", "Brussels"),
        new("Bob", "Bob", "Operator", "Antwerp"),
        new("Charlie", "Charlie", "Manager", "Brussels"),
        new("Diana", "Diana", "Finance", "Antwerp"),
        new("Admin", "Admin", "Administrator", "Ghent")
    ];

    public static DemoUser? Get(string? userId) =>
        string.IsNullOrWhiteSpace(userId) ? null : Users.FirstOrDefault(u => string.Equals(u.Id, userId, StringComparison.OrdinalIgnoreCase));
}

public static class DemoWarehouseCatalog
{
    public static readonly IReadOnlyList<string> Warehouses = ["Brussels", "Antwerp", "Ghent"];
    public static readonly IReadOnlyList<string> EventTypeNames = ["Orders", "Shipments", "Payments", "Maintenance", "System Alerts"];
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
}

public static class DemoEventFactory
{
    private static readonly Random Random = new();

    public static DemoEvent CreateRandomEvent()
    {
        var warehouse = DemoWarehouseCatalog.Warehouses[Random.Next(DemoWarehouseCatalog.Warehouses.Count)];
        var type = Enum.GetValues<DemoEventType>()[Random.Next(Enum.GetValues<DemoEventType>().Length)];
        var user = DemoUserCatalog.Users[Random.Next(DemoUserCatalog.Users.Count)];

        return type switch
        {
            DemoEventType.OrderCreated => new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, warehouse, null, null, DateTimeOffset.Now, $"Order created for {warehouse} warehouse.", false),
            DemoEventType.ShipmentDelayed => new DemoEvent(Guid.NewGuid(), DemoEventType.ShipmentDelayed, warehouse, null, null, DateTimeOffset.Now, $"Shipment delayed in {warehouse}.", false),
            DemoEventType.PaymentReceived => new DemoEvent(Guid.NewGuid(), DemoEventType.PaymentReceived, null, user.Id, null, DateTimeOffset.Now, $"Payment received for {user.Name}.", false),
            DemoEventType.MaintenanceStarted => new DemoEvent(Guid.NewGuid(), DemoEventType.MaintenanceStarted, warehouse, null, null, DateTimeOffset.Now, $"Maintenance started in {warehouse}.", false),
            DemoEventType.SystemAlert => new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, warehouse, null, "Finance", DateTimeOffset.Now, $"System alert for {warehouse}.", false),
            DemoEventType.UserNotification => new DemoEvent(Guid.NewGuid(), DemoEventType.UserNotification, null, user.Id, null, DateTimeOffset.Now, $"Personal notification for {user.Name}.", false),
            _ => new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, warehouse, null, null, DateTimeOffset.Now, "Random demo event.", false)
        };
    }

    public static IReadOnlyList<DemoEvent> BuildScenario(DemoScenario scenario)
    {
        return scenario switch
        {
            DemoScenario.NormalOperations =>
            [
                new(Guid.NewGuid(), DemoEventType.OrderCreated, "Brussels", null, null, DateTimeOffset.Now, "Order created in Brussels."),
                new(Guid.NewGuid(), DemoEventType.OrderCreated, "Antwerp", null, null, DateTimeOffset.Now, "Order created in Antwerp."),
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, "Brussels", null, null, DateTimeOffset.Now, "Shipment delayed in Brussels."),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, null, "Alice", null, DateTimeOffset.Now, "Payment received for Alice.")
            ],
            DemoScenario.BrusselsIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.ShipmentDelayed, "Brussels", null, null, DateTimeOffset.Now, "Brussels shipment delayed."),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, "Brussels", null, "Operator", DateTimeOffset.Now, "System alert in Brussels."),
                new(Guid.NewGuid(), DemoEventType.MaintenanceStarted, "Brussels", null, null, DateTimeOffset.Now, "Maintenance work started in Brussels.")
            ],
            DemoScenario.PaymentIncident =>
            [
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, null, "Alice", null, DateTimeOffset.Now, "Payment received for Alice."),
                new(Guid.NewGuid(), DemoEventType.PaymentReceived, null, "Bob", null, DateTimeOffset.Now, "Payment received for Bob."),
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, null, "Finance", DateTimeOffset.Now, "Finance alert: payment issue detected.")
            ],
            DemoScenario.UserNotification =>
            [
                new(Guid.NewGuid(), DemoEventType.UserNotification, null, "Alice", null, DateTimeOffset.Now, "Alice received a user notification.")
            ],
            DemoScenario.AlertFinance =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, null, "Finance", DateTimeOffset.Now, "Finance alert issued.")
            ],
            DemoScenario.BroadcastEmergency =>
            [
                new(Guid.NewGuid(), DemoEventType.SystemAlert, null, null, null, DateTimeOffset.Now, "Emergency broadcast to everyone.", true)
            ],
            _ => [CreateRandomEvent()]
        };
    }
}
