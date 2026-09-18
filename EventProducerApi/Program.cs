using DemoShared;
using EventProducerApi;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("rabbitmq")
        .AutoProvision()
        .DeclareExchange("demo-events");

    options.PublishAllMessages().ToRabbitExchange("demo-events");
});
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
builder.Services.AddHostedService<DemoEventProducerService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalBlazor");

app.MapPost("/simulator/scenario/{scenario}", async (string scenario, IMessageBus messageBus, ILoggerFactory loggerFactory) =>
{
    var selected = Enum.TryParse<DemoScenario>(scenario, true, out var parsed) ? parsed : DemoScenario.Orders;
    var events = DemoEventFactory.BuildScenario(selected);
    foreach (var demoEvent in events)
    {
        var message = DemoEventMessageFactory.Create(demoEvent);
        await messageBus.PublishAsync(message);
    }

    loggerFactory.CreateLogger("Simulator").LogInformation("[Producer] Scenario {Scenario} published {Count} event(s).", scenario, events.Count);
    return Results.Ok(new { scenario, count = events.Count });
});

app.Run();
