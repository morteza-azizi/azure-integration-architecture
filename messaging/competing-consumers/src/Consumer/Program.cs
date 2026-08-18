using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var id = $"consumer-{Environment.ProcessId}";
var queue = config["Queue"] ?? "notifications";
var downstreamUrl = config["DownstreamUrl"] ?? "http://localhost:5000";
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

using var http = new HttpClient { BaseAddress = new Uri(downstreamUrl) };

processor.ProcessMessageAsync += async args =>
{
    var notification = args.Message.Body.ToString();
    Console.WriteLine($"[{id}] processing {notification}");

    using var response = await http.PostAsync(
        "/notifications",
        new StringContent($"{{\"id\":\"{notification}\"}}", Encoding.UTF8, "application/json"),
        args.CancellationToken);
    response.EnsureSuccessStatusCode();

    await args.CompleteMessageAsync(args.Message);
    Console.WriteLine($"[{id}] completed {notification}");
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
