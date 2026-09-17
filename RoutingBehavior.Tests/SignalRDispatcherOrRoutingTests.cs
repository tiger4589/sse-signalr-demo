using DemoShared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SignalRApi;

namespace RoutingBehavior.Tests;

public class SignalRDispatcherOrRoutingTests
{
    [Fact]
    public async Task UserTargetedEvent_SendsToUserGroup()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<SignalRMessageDispatcher>>();
        string? capturedGroup = null;

        clients.Setup(x => x.Group(It.IsAny<string>()))
            .Callback<string>(group => capturedGroup = group)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var dispatcher = new SignalRMessageDispatcher(hubContext.Object, logger.Object);
        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.UtcNow, "Payment received for Alice", DemoEventTarget.User("Alice"));
        var message = new UserTargetedEventMessage(demoEvent, "Alice");

        await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.Equal("user:alice", capturedGroup);

        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RoleTargetedEvent_SendsToRoleGroup()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<SignalRMessageDispatcher>>();
        string? capturedGroup = null;

        clients.Setup(x => x.Group(It.IsAny<string>()))
            .Callback<string>(group => capturedGroup = group)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var dispatcher = new SignalRMessageDispatcher(hubContext.Object, logger.Object);
        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.UtcNow, "Finance alert", DemoEventTarget.Role("Finance"));
        var message = new RoleTargetedEventMessage(demoEvent, "Finance");

        await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.Equal("role:finance", capturedGroup);

        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EventTypeTargetedEvent_SendsToEventTypeGroup()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<SignalRMessageDispatcher>>();
        string? capturedGroup = null;

        clients.Setup(x => x.Group(It.IsAny<string>()))
            .Callback<string>(group => capturedGroup = group)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var dispatcher = new SignalRMessageDispatcher(hubContext.Object, logger.Object);
        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, DateTimeOffset.UtcNow, "Order update for subscribers", DemoEventTarget.EventType("Orders"));
        var message = new EventTypeTargetedEventMessage(demoEvent, "Orders");

        await dispatcher.DispatchAsync(message, CancellationToken.None);

        Assert.Equal("event-type:orders", capturedGroup);

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
        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.UtcNow, "Broadcast emergency", DemoEventTarget.Broadcast());
        var message = new BroadcastEventMessage(demoEvent);

        await dispatcher.DispatchAsync(message, CancellationToken.None);

        clients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
