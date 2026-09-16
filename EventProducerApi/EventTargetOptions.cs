namespace EventProducerApi;

public sealed class EventTargetOptions
{
    public const string SectionName = "EventTargets";

    public string SseApiUrl { get; set; } = "https://localhost:9003";
    public string SignalRApiUrl { get; set; } = "https://localhost:9004";
}
