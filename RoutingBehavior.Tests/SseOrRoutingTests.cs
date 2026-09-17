using DemoShared;
using SseApi;

namespace RoutingBehavior.Tests;

public class SseOrRoutingTests
{
    [Fact]
    public void WarehouseMatch_DeliversWithoutEventTypeSubscription()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-warehouse", "Alice");

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            "Brussels",
            null,
            null,
            DateTimeOffset.UtcNow,
            "Brussels shipment delayed");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void EventTypeMatch_DeliversAcrossDifferentWarehouse()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-event-type", "Bob");
        registry.UpdateEventType("conn-event-type", "Shipments", selected: true);

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            "Brussels",
            null,
            null,
            DateTimeOffset.UtcNow,
            "Brussels shipment delayed");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void PureGlobalOr_UserTargetedEventAlsoDeliversOnWarehouseMatch()
    {
        var registry = new SseConnectionRegistry();
        var alice = registry.Register("conn-alice", "Alice");
        var charlie = registry.Register("conn-charlie", "Charlie");

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            "Brussels",
            "Alice",
            null,
            DateTimeOffset.UtcNow,
            "Brussels event with explicit user target");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(alice, targets);
        Assert.Contains(charlie, targets);
    }

    [Fact]
    public void MultipleMatchingAxes_DoNotDuplicateSingleConnection()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-dedup", "Alice");
        registry.UpdateEventType("conn-dedup", "Orders", selected: true);

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.OrderCreated,
            "Brussels",
            "Alice",
            "Operator",
            DateTimeOffset.UtcNow,
            "Order created in Brussels");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Equal(1, targets.Count(target => ReferenceEquals(target, state)));
    }
}
