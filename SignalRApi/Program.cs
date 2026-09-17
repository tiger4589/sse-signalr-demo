using DemoShared;
using SignalRApi;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Host.UseWolverine(options =>
{
    options.UseRabbitMqUsingNamedConnection("rabbitmq")
        .AutoProvision()
        .DeclareExchange("demo-events", exchange => exchange.BindQueue("signalr-demo-events"));

    options.ListenToRabbitQueue("signalr-demo-events");
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
builder.Services.AddSignalR();
builder.Services.AddSingleton<SignalRMessageDispatcher>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalBlazor");

app.MapHub<SignalRDemoHub>("/demohub");

app.MapGet("/healthcheck", () => Results.Ok(new { status = "ok", service = "SignalR" }));
app.MapGet("/demo-users", () => DemoUserCatalog.Users);
app.MapGet("/demo-settings", () => Results.Ok(new
{
    eventTypes = Enum.GetNames<DemoEventType>()
}));

app.Run();
