using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var queue = config["Queue"] ?? "notifications";
var count = int.TryParse(config["Count"], out var n) ? n : 20;
var connection = config["ServiceBusConnection"];

if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("Set ServiceBusConnection in appsettings.json.");
}

await using var client = new ServiceBusClient(connection);
await using var sender = client.CreateSender(queue);

for (var i = 1; i <= count; i++)
{
    await sender.SendMessageAsync(new ServiceBusMessage($"notification-{i}"));
    Console.WriteLine($"queued notification-{i}");
}

Console.WriteLine($"done. queued {count} notifications on {queue}");
