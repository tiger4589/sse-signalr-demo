using DemoShared;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApi;

public sealed class SignalRDemoHub : Hub
{
    private readonly ILogger<SignalRDemoHub> _logger;

    public SignalRDemoHub(ILogger<SignalRDemoHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        var user = DemoUserCatalog.Get(userId);

        if (!string.IsNullOrWhiteSpace(user?.Role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.Role(user.Role));
        }

        _logger.LogInformation("[SignalR] Connection established for {UserId} on {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[SignalR] Connection closed for {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new HubException("Event type is required.");
        }

        var normalizedEventType = eventType.Trim();
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.EventType(normalizedEventType));

        _logger.LogInformation("[SignalR] Connection {ConnectionId} subscribed to {EventType}", Context.ConnectionId, normalizedEventType);
    }

    public async Task UnsubscribeFromEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new HubException("Event type is required.");
        }

        var normalizedEventType = eventType.Trim();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.EventType(normalizedEventType));

        _logger.LogInformation("[SignalR] Connection {ConnectionId} unsubscribed from {EventType}", Context.ConnectionId, normalizedEventType);
    }

    public Task<string> GetConnectionInfo()
    {
        var userId = GetCurrentUserId();
        return Task.FromResult($"{userId}:{Context.ConnectionId}");
    }

    private string GetCurrentUserId() =>
        string.IsNullOrWhiteSpace(Context.UserIdentifier) ? "Alice" : Context.UserIdentifier;
}
