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

        var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(demoEvent.UserId))
        {
            groups.Add(SignalRGroupNames.User(demoEvent.UserId));
        }

        if (!string.IsNullOrWhiteSpace(demoEvent.TargetRole))
        {
            groups.Add(SignalRGroupNames.Role(demoEvent.TargetRole));
        }

        var eventType = demoEvent.Type.ToFriendlyName();
        groups.Add(SignalRGroupNames.EventType(eventType));

        var targetGroups = groups.ToList();
        _logger.LogInformation("[SignalR] Sending {EventType} to groups:{Groups}", demoEvent.Type, string.Join(", ", targetGroups));
        await _hubContext.Clients.Groups(targetGroups).SendAsync("ReceiveEvent", demoEvent, cancellationToken);
    }
}
