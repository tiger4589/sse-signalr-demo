using DemoShared;
using SseApi;

namespace RoutingBehavior.Tests;

public class SseOrRoutingTests
{
    [Fact]
    public void RoleTarget_ReturnsMatchingRoleConnections()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-role", "Diana");

        var targets = registry.GetConnectionsForRole("Finance").ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void EventTypeTarget_ReturnsSubscribedConnections()
    {
        var registry = new SseConnectionRegistry();
        var state = registry.Register("conn-event-type", "Bob");
        registry.UpdateEventType("conn-event-type", "Shipments", selected: true);

        var targets = registry.GetConnectionsForEventType("Shipments").ToList();

        Assert.Contains(state, targets);
    }

    [Fact]
    public void UserTarget_ReturnsOnlyMatchingUserConnections()
    {
        var registry = new SseConnectionRegistry();
        var alice = registry.Register("conn-alice", "Alice");
        var charlie = registry.Register("conn-charlie", "Charlie");

        var targets = registry.GetConnectionsForUser("Alice").ToList();

        Assert.Contains(alice, targets);
        Assert.DoesNotContain(charlie, targets);
    }

    [Fact]
    public void BroadcastTarget_ReturnsAllConnections()
    {
        var registry = new SseConnectionRegistry();
        var alice = registry.Register("conn-alice", "Alice");
        var bob = registry.Register("conn-bob", "Bob");

        var targets = registry.GetAllConnections().ToList();

        Assert.Contains(alice, targets);
        Assert.Contains(bob, targets);
        Assert.Equal(2, targets.Count);
    }
}
