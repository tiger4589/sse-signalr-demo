using System.Net.Http.Json;
using DemoShared;
using Microsoft.Extensions.Options;

namespace EventProducerApi;

public sealed class EventFanOutPublisher
{
    private readonly HttpClient _httpClient;
    private readonly EventTargetOptions _options;
    private readonly ILogger<EventFanOutPublisher> _logger;

    public EventFanOutPublisher(HttpClient httpClient, IOptions<EventTargetOptions> options, ILogger<EventFanOutPublisher> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(DemoEvent demoEvent, CancellationToken cancellationToken)
    {
        var sseTask = PostEventAsync(_options.SseApiUrl, "SSE", demoEvent, cancellationToken);
        var signalRTask = PostEventAsync(_options.SignalRApiUrl, "SignalR", demoEvent, cancellationToken);
        await Task.WhenAll(sseTask, signalRTask);
    }

    private async Task PostEventAsync(string baseUrl, string targetName, DemoEvent demoEvent, CancellationToken cancellationToken)
    {
        var targetUrl = $"{baseUrl.TrimEnd('/')}/internal/events";
        using var response = await _httpClient.PostAsJsonAsync(targetUrl, demoEvent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Event delivery to {targetName} failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        _logger.LogInformation("[Producer] Sent {EventType} to {Target}", demoEvent.Type, targetName);
    }
}
