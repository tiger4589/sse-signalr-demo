using DemoShared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using SignalRApi;

namespace RoutingBehavior.Tests;

public class SignalRDispatcherOrRoutingTests
{
    [Fact]
    public async Task UserTargetedEvent_SendsToUser()
    {
        var clients = new Mock<IHubClients>();
        var hubContext = new Mock<IHubContext<SignalRDemoHub>>();
        var proxy = new Mock<IClientProxy>();
        var logger = new Mock<ILogger<UserTargetedSignalRConsumer>>();
        string? capturedUserId = null;

        clients.Setup(x => x.User(It.IsAny<string>()))
            .Callback<string>(userId => capturedUserId = userId)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.PaymentReceived, DateTimeOffset.UtcNow, "Payment received for Alice", DemoEventTarget.User("Alice"));
        var message = new UserTargetedEventMessage(demoEvent, "Alice");

        await UserTargetedSignalRConsumer.Handle(message, hubContext.Object, logger.Object, CancellationToken.None);

        Assert.Equal("Alice", capturedUserId);

        clients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
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
        var logger = new Mock<ILogger<RoleTargetedSignalRConsumer>>();
        string? capturedGroup = null;

        clients.Setup(x => x.Group(It.IsAny<string>()))
            .Callback<string>(group => capturedGroup = group)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.UtcNow, "Finance alert", DemoEventTarget.Role("Finance"));
        var message = new RoleTargetedEventMessage(demoEvent, "Finance");

        await RoleTargetedSignalRConsumer.Handle(message, hubContext.Object, logger.Object, CancellationToken.None);

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
        var logger = new Mock<ILogger<EventTypeTargetedSignalRConsumer>>();
        string? capturedGroup = null;

        clients.Setup(x => x.Group(It.IsAny<string>()))
            .Callback<string>(group => capturedGroup = group)
            .Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.OrderCreated, DateTimeOffset.UtcNow, "Order update for subscribers", DemoEventTarget.EventType("Orders"));
        var message = new EventTypeTargetedEventMessage(demoEvent, "Orders");

        await EventTypeTargetedSignalRConsumer.Handle(message, hubContext.Object, logger.Object, CancellationToken.None);

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
        var logger = new Mock<ILogger<BroadcastSignalRConsumer>>();

        clients.SetupGet(x => x.All).Returns(proxy.Object);
        hubContext.SetupGet(x => x.Clients).Returns(clients.Object);

        var demoEvent = new DemoEvent(Guid.NewGuid(), DemoEventType.SystemAlert, DateTimeOffset.UtcNow, "Broadcast emergency", DemoEventTarget.Broadcast());
        var message = new BroadcastEventMessage(demoEvent);

        await BroadcastSignalRConsumer.Handle(message, hubContext.Object, logger.Object, CancellationToken.None);

        clients.Verify(x => x.Group(It.IsAny<string>()), Times.Never);
        proxy.Verify(
            x => x.SendCoreAsync(
                "ReceiveEvent",
                It.Is<object?[]>(args => args.Length == 1 && Equals(args[0], demoEvent)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
