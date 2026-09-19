using System.Collections.Concurrent;
using DemoShared;

namespace SseApi;

public sealed class SseConnectionRegistry
{
    private readonly ConcurrentDictionary<string, SseConnectionState> _connections = new(StringComparer.OrdinalIgnoreCase);

    public SseConnectionState Register(string connectionKey, string userId)
    {
        var state = _connections.GetOrAdd(connectionKey, _ => new SseConnectionState { UserId = userId });
        state.UserId = userId;
        state.Roles.Clear();

        var userRole = DemoUserCatalog.Get(userId)?.Role;
        if (!string.IsNullOrWhiteSpace(userRole))
        {
            state.Roles.Add(Normalize(userRole));
        }

        return state;
    }

    public void Remove(string connectionKey)
    {
        if (_connections.TryRemove(connectionKey, out var state))
        {
            state.Complete();
        }
    }

    public IEnumerable<SseConnectionState> GetAllConnections() => _connections.Values;

    public IEnumerable<SseConnectionState> GetConnectionsForUser(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        foreach (var state in _connections.Values)
        {
            if (string.Equals(state.UserId, userId, StringComparison.OrdinalIgnoreCase))
            {
                yield return state;
            }
        }
    }

    public IEnumerable<SseConnectionState> GetConnectionsForRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        var normalizedRole = Normalize(role);

        foreach (var state in _connections.Values)
        {
            if (state.Roles.Contains(normalizedRole))
            {
                yield return state;
            }
        }
    }

    public IEnumerable<SseConnectionState> GetConnectionsForEventType(string eventType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);

        foreach (var state in _connections.Values)
        {
            if (state.EventTypes.Contains(eventType))
            {
                yield return state;
            }
        }
    }

    public void UpdateEventType(string connectionKey, string eventType, bool selected)
    {
        if (!_connections.TryGetValue(connectionKey, out var state))
        {
            return;
        }

        var normalizedEventType = Normalize(eventType);
        if (selected)
        {
            state.EventTypes.Add(normalizedEventType);
        }
        else
        {
            state.EventTypes.Remove(normalizedEventType);
        }
    }

    private static string Normalize(string value) => value.Trim();
}
