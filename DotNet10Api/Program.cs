using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/events", (CancellationToken cancellationToken) =>
{
    async IAsyncEnumerable<int> GetCounter(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var counter = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return counter++;
            await Task.Delay(1000, cancellationToken);
        }
    }

    return TypedResults.ServerSentEvents(GetCounter(cancellationToken));
});

app.Run();


