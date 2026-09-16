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

    public async Task DispatchAsync(DemoEvent demoEvent, CancellationToken cancellationToken)
    {
        if (demoEvent.BroadcastToEveryone)
        {
            _logger.LogInformation("[SignalR] Broadcasting {EventType} to all", demoEvent.Type);
            await _hubContext.Clients.All.SendAsync("ReceiveEvent", demoEvent, cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(demoEvent.UserId))
        {
            _logger.LogInformation("[SignalR] Sending {EventType} to user:{UserId}", demoEvent.Type, demoEvent.UserId);
            await _hubContext.Clients.Group(SignalRGroupNames.User(demoEvent.UserId)).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(demoEvent.TargetRole))
        {
            _logger.LogInformation("[SignalR] Sending {EventType} to role:{Role}", demoEvent.Type, demoEvent.TargetRole);
            await _hubContext.Clients.Group(SignalRGroupNames.Role(demoEvent.TargetRole)).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
            return;
        }

        var eventType = demoEvent.Type.ToFriendlyName();
        if (!string.IsNullOrWhiteSpace(demoEvent.Warehouse))
        {
            var group = SignalRGroupNames.WarehouseEvent(demoEvent.Warehouse, eventType);
            _logger.LogInformation("[SignalR] Sending {EventType} to group:{Group}", demoEvent.Type, group);
            await _hubContext.Clients.Group(group).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
            return;
        }

        var eventTypeGroup = SignalRGroupNames.EventType(eventType);
        _logger.LogInformation("[SignalR] Sending {EventType} to group:{Group}", demoEvent.Type, eventTypeGroup);
        await _hubContext.Clients.Group(eventTypeGroup).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
    }
}
