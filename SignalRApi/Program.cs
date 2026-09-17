using DemoShared;
using SignalRApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
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

app.MapPost("/internal/events", async (DemoEvent demoEvent, SignalRMessageDispatcher dispatcher, CancellationToken cancellationToken) =>
{
    await dispatcher.DispatchAsync(demoEvent, cancellationToken);
    return Results.Accepted();
});

app.Run();
