namespace SignalRApi;

public static class SignalRGroupNames
{
    public static string Role(string role) => $"role:{Normalize(role)}";
    public static string EventType(string eventType) => $"event-type:{Normalize(eventType)}";

    private static string Normalize(string value) => value.Trim().Replace(' ', '-').ToLowerInvariant();
}
