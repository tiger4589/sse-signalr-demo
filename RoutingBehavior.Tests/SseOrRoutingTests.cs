using DemoShared;
using SseApi;

namespace RoutingBehavior.Tests;

public class SseOrRoutingTests
{
    [Fact]
    public void RoleMatch_DeliversWithoutEventTypeSubscription()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-role", "Diana");

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.SystemAlert,
            null,
            "Finance",
            DateTimeOffset.UtcNow,
            "Finance alert");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void EventTypeMatch_DeliversAcrossDifferentRole()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-event-type", "Bob");
        registry.UpdateEventType("conn-event-type", "Shipments", selected: true);

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            null,
            null,
            DateTimeOffset.UtcNow,
            "Shipment delayed");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void UserTargetedEventAlsoDeliversToMatchingUser()
    {
        var registry = new SseConnectionRegistry();
        var alice = registry.Register("conn-alice", "Alice");
        var charlie = registry.Register("conn-charlie", "Charlie");

        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            "Alice",
            null,
            DateTimeOffset.UtcNow,
            "Shipment event with explicit user target");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Contains(alice, targets);
        Assert.DoesNotContain(charlie, targets);
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
            "Alice",
            "Operator",
            DateTimeOffset.UtcNow,
            "Order created for Alice");

        var targets = registry.GetTargets(demoEvent).ToList();

        Assert.Equal(1, targets.Count(target => ReferenceEquals(target, state)));
    }
}
