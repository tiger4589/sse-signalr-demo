using DemoShared;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApi;

public sealed class SignalRMessageDispatcher
{
    private readonly IHubContext<SignalRDemoHub> _hubContext;
    private readonly ILogger<SignalRMessageDispatcher> _logger;

    public SignalRMessageDispatcher(IHubContext<SignalRDemoHub> hubContext, ILogger<SignalRMessageDispatcher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task DispatchAsync(UserTargetedEventMessage message, CancellationToken cancellationToken) =>
        SendToGroupAsync(message.Event, SignalRGroupNames.User(message.UserId), cancellationToken);

    public Task DispatchAsync(RoleTargetedEventMessage message, CancellationToken cancellationToken) =>
        SendToGroupAsync(message.Event, SignalRGroupNames.Role(message.Role), cancellationToken);

    public Task DispatchAsync(EventTypeTargetedEventMessage message, CancellationToken cancellationToken) =>
        SendToGroupAsync(message.Event, SignalRGroupNames.EventType(message.EventType), cancellationToken);

    public async Task DispatchAsync(BroadcastEventMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[SignalR] Broadcasting {EventType} to all", message.Event.Type);
        await _hubContext.Clients.All.SendAsync("ReceiveEvent", message.Event, cancellationToken);
    }

    private async Task SendToGroupAsync(DemoEvent demoEvent, string groupName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[SignalR] Sending {EventType} to group:{Group}", demoEvent.Type, groupName);
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
    }
}
