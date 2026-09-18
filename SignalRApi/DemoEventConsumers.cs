using DemoShared;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApi;

public sealed class UserTargetedSignalRConsumer
{
    public static async Task Handle(
        UserTargetedEventMessage message,
        IHubContext<SignalRDemoHub> hubContext,
        ILogger<UserTargetedSignalRConsumer> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[SignalR] Sending {EventType} to user:{UserId}", message.Event.Type, message.UserId);
        await hubContext.Clients.User(message.UserId).SendAsync("ReceiveEvent", message.Event, cancellationToken);
    }
}

public sealed class RoleTargetedSignalRConsumer
{
    public static async Task Handle(
        RoleTargetedEventMessage message,
        IHubContext<SignalRDemoHub> hubContext,
        ILogger<RoleTargetedSignalRConsumer> logger,
        CancellationToken cancellationToken)
    {
        var groupName = SignalRGroupNames.Role(message.Role);
        logger.LogInformation("[SignalR] Sending {EventType} to group:{Group}", message.Event.Type, groupName);
        await hubContext.Clients.Group(groupName).SendAsync("ReceiveEvent", message.Event, cancellationToken);
    }
}

public sealed class EventTypeTargetedSignalRConsumer
{
    public static async Task Handle(
        EventTypeTargetedEventMessage message,
        IHubContext<SignalRDemoHub> hubContext,
        ILogger<EventTypeTargetedSignalRConsumer> logger,
        CancellationToken cancellationToken)
    {
        var groupName = SignalRGroupNames.EventType(message.EventType);
        logger.LogInformation("[SignalR] Sending {EventType} to group:{Group}", message.Event.Type, groupName);
        await hubContext.Clients.Group(groupName).SendAsync("ReceiveEvent", message.Event, cancellationToken);
    }
}

public sealed class BroadcastSignalRConsumer
{
    public static async Task Handle(
        BroadcastEventMessage message,
        IHubContext<SignalRDemoHub> hubContext,
        ILogger<BroadcastSignalRConsumer> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("[SignalR] Broadcasting {EventType} to all", message.Event.Type);
        await hubContext.Clients.All.SendAsync("ReceiveEvent", message.Event, cancellationToken);
    }
}
