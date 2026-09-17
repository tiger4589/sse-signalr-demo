using DemoShared;
using EventProducerApi;
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
builder.Services.Configure<EventTargetOptions>(builder.Configuration.GetSection(EventTargetOptions.SectionName));
builder.Services.AddHostedService<DemoEventProducerService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalBlazor");

app.MapGet("/healthcheck", () => Results.Ok(new { status = "ok", service = "EventProducer" }));
app.MapGet("/demo-users", () => DemoUserCatalog.Users);
app.MapGet("/demo-settings", () => Results.Ok(new
{
    eventTypes = Enum.GetNames<DemoEventType>()
}));

app.MapPost("/simulator/scenario/{scenario}", async (string scenario, EventFanOutPublisher publisher, ILoggerFactory loggerFactory, CancellationToken cancellationToken) =>
{
    var selected = Enum.TryParse<DemoScenario>(scenario, true, out var parsed) ? parsed : DemoScenario.NormalOperations;
    var events = DemoEventFactory.BuildScenario(selected);
    foreach (var demoEvent in events)
    {
        await publisher.PublishAsync(demoEvent, cancellationToken);
    }

    loggerFactory.CreateLogger("Simulator").LogInformation("[Producer] Scenario {Scenario} published {Count} event(s).", scenario, events.Count);
    return Results.Ok(new { scenario, count = events.Count });
});

app.MapPost("/simulator/random", async (EventFanOutPublisher publisher, CancellationToken cancellationToken) =>
{
    var demoEvent = DemoEventFactory.CreateRandomEvent();
    await publisher.PublishAsync(demoEvent, cancellationToken);
    return Results.Ok(demoEvent);
});

app.Run();
