using DemoShared;
using System.Net.ServerSentEvents;
using SseApi;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseWolverine();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalBlazor", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin))
                {
                    return false;
                }

                return Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                       uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<SseConnectionRegistry>();
builder.Services.AddSingleton<SseMessageDispatcher>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalBlazor");

app.MapGet("/events", (HttpContext httpContext, SseConnectionRegistry registry) =>
{
    var userId = httpContext.Request.Query["userId"].ToString();
    var connectionId = httpContext.Request.Query["connectionId"].ToString();
    if (string.IsNullOrWhiteSpace(connectionId))
    {
        connectionId = Guid.NewGuid().ToString();
    }

    var state = registry.Register(connectionId, userId);

    async IAsyncEnumerable<SseItem<DemoEvent>> StreamEvents(
        SseConnectionState streamState,
        string streamConnectionId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var sseItem in streamState.ReadEventsAsync(cancellationToken))
            {
                yield return sseItem;
            }
        }
        finally
        {
            registry.Remove(streamConnectionId);
        }
    }

    return TypedResults.ServerSentEvents(StreamEvents(state, connectionId, httpContext.RequestAborted));
});

app.MapPost("/events/subscriptions", (string userId, string? eventType, SseConnectionRegistry registry, HttpContext httpContext) =>
{
    var connectionId = httpContext.Request.Query["connectionId"].ToString();
    if (string.IsNullOrWhiteSpace(connectionId))
    {
        return Results.BadRequest(new { error = "Missing connectionId." });
    }

    if (string.IsNullOrWhiteSpace(eventType))
    {
        return Results.BadRequest(new { error = "Provide eventType." });
    }

    registry.UpdateEventType(connectionId, eventType, true);

    return Results.Ok(new { userId, eventType, status = "subscribed" });
});

app.MapDelete("/events/subscriptions", (string userId, string? eventType, SseConnectionRegistry registry, HttpContext httpContext) =>
{
    var connectionId = httpContext.Request.Query["connectionId"].ToString();
    if (string.IsNullOrWhiteSpace(connectionId))
    {
        return Results.BadRequest(new { error = "Missing connectionId." });
    }

    if (string.IsNullOrWhiteSpace(eventType))
    {
        return Results.BadRequest(new { error = "Provide eventType." });
    }

    registry.UpdateEventType(connectionId, eventType, false);

    return Results.Ok(new { userId, eventType, status = "unsubscribed" });
});

#region HiddenForClarity

app.MapGet("/demo-users", () => DemoUserCatalog.Users);
app.MapGet("/demo-settings", () => Results.Ok(new
{
    eventTypes = Enum.GetNames<DemoEventType>()
}));

#endregion

app.Run();
