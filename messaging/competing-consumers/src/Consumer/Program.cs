using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var id = $"consumer-{Environment.ProcessId}";
var queue = config["Queue"] ?? "notifications";
var connection = config["ServiceBusConnection"];

if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("Set ServiceBusConnection in appsettings.json.");
}

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

await using var client = new ServiceBusClient(connection);
await using var processor = client.CreateProcessor(queue, new ServiceBusProcessorOptions
{
    AutoCompleteMessages = false,
    MaxConcurrentCalls = 1
});

processor.ProcessMessageAsync += async args =>
{
    var notification = args.Message.Body.ToString();
    Console.WriteLine($"[{id}] sending {notification}");
    await Task.Delay(200, args.CancellationToken);
    await args.CompleteMessageAsync(args.Message);
};

processor.ProcessErrorAsync += args =>
{
    Console.WriteLine($"[{id}] error: {args.Exception.Message}");
    return Task.CompletedTask;
};

Console.WriteLine($"[{id}] listening on {queue}");
await processor.StartProcessingAsync(shutdown.Token);

try
{
    await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
}
catch (OperationCanceledException)
{
}

await processor.StopProcessingAsync();
Console.WriteLine($"[{id}] stopped");
