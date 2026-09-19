#region Dangerous Zone, Do Not Enter

using System.Runtime.CompilerServices;

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
#endregion

app.MapGet("/events", (CancellationToken cancellationToken) =>
{
    async IAsyncEnumerable<int> GetCounter(
        [EnumeratorCancellation] CancellationToken internalCancellationToken)
    {
        var counter = 0;
        while (!internalCancellationToken.IsCancellationRequested)
        {
            yield return counter++;
            try
            {
                await Task.Delay(1000, internalCancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    return TypedResults.ServerSentEvents(GetCounter(cancellationToken));
});

app.Run();


