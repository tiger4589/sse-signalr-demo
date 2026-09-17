using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Threading.Channels;
using DemoShared;

namespace SseApi;

public sealed class SseConnectionState
{
    public string UserId { get; set; } = string.Empty;
    public HashSet<string> EventTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
    private readonly Channel<SseItem<DemoEvent>> _events = Channel.CreateUnbounded<SseItem<DemoEvent>>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public bool TryQueueEvent(DemoEvent demoEvent) => _events.Writer.TryWrite(
        new SseItem<DemoEvent>(demoEvent, "message")
        {
            EventId = demoEvent.Id.ToString()
        });

    public IAsyncEnumerable<SseItem<DemoEvent>> ReadEventsAsync(CancellationToken cancellationToken = default) =>
        _events.Reader.ReadAllAsync(cancellationToken);

    public void Complete() => _events.Writer.TryComplete();
}

public sealed class SseConnectionRegistry
{
    private readonly ConcurrentDictionary<string, SseConnectionState> _connections = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new(StringComparer.OrdinalIgnoreCase);

    public SseConnectionState Register(string connectionKey, string userId)
    {
        var state = _connections.GetOrAdd(connectionKey, _ => new SseConnectionState { UserId = userId });
        state.UserId = userId;

        _userConnections.AddOrUpdate(userId,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { connectionKey },
            (_, ids) =>
            {
                ids.Add(connectionKey);
                return ids;
            });

        return state;
    }

    public void Remove(string connectionKey)
    {
        if (_connections.TryRemove(connectionKey, out var state))
        {
            state.Complete();

            if (_userConnections.TryGetValue(state.UserId, out var ids))
            {
                ids.Remove(connectionKey);
                if (ids.Count == 0)
                {
                    _userConnections.TryRemove(state.UserId, out _);
                }
            }
        }
    }

    public IEnumerable<SseConnectionState> GetTargets(DemoEvent demoEvent)
    {
        var eventType = demoEvent.Type.ToFriendlyName();

        foreach (var state in _connections.Values)
        {
            var user = DemoUserCatalog.Get(state.UserId);
            var matchesUser = !string.IsNullOrWhiteSpace(demoEvent.UserId) &&
                              string.Equals(state.UserId, demoEvent.UserId, StringComparison.OrdinalIgnoreCase);
            var matchesRole = !string.IsNullOrWhiteSpace(demoEvent.TargetRole) &&
                              user is not null &&
                              string.Equals(user.Role, demoEvent.TargetRole, StringComparison.OrdinalIgnoreCase);
            var matchesEventType = state.EventTypes.Contains(eventType);

            if (demoEvent.BroadcastToEveryone || matchesUser || matchesRole || matchesEventType)
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

        if (selected)
        {
            state.EventTypes.Add(eventType);
        }
        else
        {
            state.EventTypes.Remove(eventType);
        }
    }
}
