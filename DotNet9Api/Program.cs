var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalBlazor");

app.MapGet("/events", async context =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    var cancellation = context.RequestAborted;

    var counter = 0;

    while (!cancellation.IsCancellationRequested)
    {
        try
        {
            await context.Response.WriteAsync(
            $"data: {counter++}\n\n",
            cancellation);

            await context.Response.Body.FlushAsync(cancellation);

            await Task.Delay(1000, cancellation);
        }
        catch (TaskCanceledException)
        {
            break;
        }
    }
});

app.Run();


