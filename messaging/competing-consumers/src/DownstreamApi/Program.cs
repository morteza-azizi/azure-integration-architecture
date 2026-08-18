var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var delayMs = app.Configuration.GetValue("Downstream:DelayMs", 0);
var maxConcurrent = app.Configuration.GetValue("Downstream:MaxConcurrentRequests", 2);
var gate = new SemaphoreSlim(maxConcurrent, maxConcurrent);

app.MapPost("/notifications", async (Notification notification) =>
{
    app.Logger.LogInformation("Received {Id}", notification.Id);

    await gate.WaitAsync();
    try
    {
        if (delayMs > 0)
        {
            await Task.Delay(delayMs);
        }

        return Results.Ok();
    }
    finally
    {
        gate.Release();
    }
});

app.Run();

record Notification(string Id);
