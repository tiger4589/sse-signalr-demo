using DemoShared;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApi;

public sealed class SignalRDemoHub : Hub
{
    private readonly ILogger<SignalRDemoHub> _logger;
    private static readonly object SubscriptionStateKey = new();

    public SignalRDemoHub(ILogger<SignalRDemoHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = "Alice";
        }

        var state = new SignalRSubscriptionState
        {
            UserId = userId
        };

        var user = DemoUserCatalog.Get(userId);
        if (user is not null)
        {
            state.Role = user.Role;
            state.Warehouses.Add(user.Warehouse);
        }

        Context.Items[SubscriptionStateKey] = state;
        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.User(state.UserId));

        if (!string.IsNullOrWhiteSpace(state.Role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.Role(state.Role));
        }

        _logger.LogInformation("[SignalR] Connection established for {UserId} on {ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("[SignalR] Connection closed for {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SetUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new HubException("User id is required.");
        }

        var state = GetOrCreateState();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.User(state.UserId));
        if (!string.IsNullOrWhiteSpace(state.Role))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.Role(state.Role));
        }

        state.UserId = userId.Trim();
        var user = DemoUserCatalog.Get(userId);
        state.Role = user?.Role;

        if (user is not null)
        {
            state.Warehouses.Add(user.Warehouse);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.User(state.UserId));
        if (!string.IsNullOrWhiteSpace(state.Role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.Role(state.Role));
        }

        _logger.LogInformation("[SignalR] User set to {UserId} ({Role})", userId, user?.Role ?? "unknown");
    }

    public async Task JoinWarehouse(string warehouse)
    {
        if (string.IsNullOrWhiteSpace(warehouse))
        {
            throw new HubException("Warehouse is required.");
        }

        var state = GetOrCreateState();
        var normalizedWarehouse = warehouse.Trim();
        if (!state.Warehouses.Add(normalizedWarehouse))
        {
            return;
        }

        foreach (var eventType in state.EventTypes)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.WarehouseEvent(normalizedWarehouse, eventType));
        }

        _logger.LogInformation("[SignalR] {UserId} joined warehouse:{Warehouse}", state.UserId, normalizedWarehouse);
    }

    public async Task LeaveWarehouse(string warehouse)
    {
        if (string.IsNullOrWhiteSpace(warehouse))
        {
            throw new HubException("Warehouse is required.");
        }

        var state = GetOrCreateState();
        var normalizedWarehouse = warehouse.Trim();
        if (!state.Warehouses.Remove(normalizedWarehouse))
        {
            return;
        }

        foreach (var eventType in state.EventTypes)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.WarehouseEvent(normalizedWarehouse, eventType));
        }

        _logger.LogInformation("[SignalR] {UserId} left warehouse:{Warehouse}", state.UserId, normalizedWarehouse);
    }

    public async Task SubscribeToEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new HubException("Event type is required.");
        }

        var state = GetOrCreateState();
        var normalizedEventType = eventType.Trim();
        if (!state.EventTypes.Add(normalizedEventType))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.EventType(normalizedEventType));
        foreach (var warehouse in state.Warehouses)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SignalRGroupNames.WarehouseEvent(warehouse, normalizedEventType));
        }

        _logger.LogInformation("[SignalR] {UserId} subscribed to {EventType}", state.UserId, normalizedEventType);
    }

    public async Task UnsubscribeFromEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new HubException("Event type is required.");
        }

        var state = GetOrCreateState();
        var normalizedEventType = eventType.Trim();
        if (!state.EventTypes.Remove(normalizedEventType))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.EventType(normalizedEventType));
        foreach (var warehouse in state.Warehouses)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, SignalRGroupNames.WarehouseEvent(warehouse, normalizedEventType));
        }

        _logger.LogInformation("[SignalR] {UserId} unsubscribed from {EventType}", state.UserId, normalizedEventType);
    }

    public Task<string> GetConnectionInfo()
    {
        var state = GetOrCreateState();
        return Task.FromResult($"{state.UserId}:{Context.ConnectionId}");
    }

    private SignalRSubscriptionState GetOrCreateState()
    {
        if (Context.Items.TryGetValue(SubscriptionStateKey, out var stateValue) && stateValue is SignalRSubscriptionState existingState)
        {
            return existingState;
        }

        var state = new SignalRSubscriptionState { UserId = "Alice" };
        Context.Items[SubscriptionStateKey] = state;
        return state;
    }
}

public sealed class SignalRSubscriptionState
{
    public string UserId { get; set; } = string.Empty;
    public string? Role { get; set; }
    public HashSet<string> Warehouses { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> EventTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
}
