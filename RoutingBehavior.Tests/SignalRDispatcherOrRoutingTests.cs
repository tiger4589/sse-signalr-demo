using DemoShared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SignalRApi;

namespace RoutingBehavior.Tests;

public class SignalRDispatcherOrRoutingTests
{
    [Fact]
    public async Task NonBroadcastEvent_SendsToUnionOfMatchedGroups()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<SignalRMessageDispatcher>>();
        IReadOnlyList<string>? capturedGroups = null;

        clients.Setup(x => x.Groups(It.IsAny<IReadOnlyList<string>>()))
            .Callback<IReadOnlyList<string>>(groups => capturedGroups = groups.ToList())
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var dispatcher = new SignalRMessageDispatcher(hubContext.Object, logger.Object);
        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.ShipmentDelayed,
            "Alice",
            "Operator",
            DateTimeOffset.UtcNow,
            "Shipment delayed");

        await dispatcher.DispatchAsync(demoEvent, CancellationToken.None);

        Assert.NotNull(capturedGroups);
        Assert.Equal(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "user:alice",
                "role:operator",
                "event-type:shipments"
            },
            capturedGroups!.ToHashSet(StringComparer.OrdinalIgnoreCase));

        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BroadcastEvent_SendsToAll()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<SignalRMessageDispatcher>>();

        clients.SetupGet(x => x.All).Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var dispatcher = new SignalRMessageDispatcher(hubContext.Object, logger.Object);
        var demoEvent = new DemoEvent(
            Guid.NewGuid(),
            DemoEventType.SystemAlert,
            null,
            null,
            DateTimeOffset.UtcNow,
            "Broadcast emergency",
            BroadcastToEveryone: true);

        await dispatcher.DispatchAsync(demoEvent, CancellationToken.None);

        clients.Verify(x => x.Groups(It.IsAny<IReadOnlyList<string>>()), Times.Never);
        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
