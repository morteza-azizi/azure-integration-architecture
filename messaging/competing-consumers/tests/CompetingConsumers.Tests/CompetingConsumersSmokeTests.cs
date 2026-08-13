using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Xunit;

namespace CompetingConsumers.Tests;

public class CompetingConsumersSmokeTests
{
    [Fact]
    public async Task TwoConsumers_ShareTheSameQueue()
    {
        await using var emulator = new ServiceBusEmulatorFixture();
        await emulator.StartAsync();

        const int messageCount = 20;
        await using var client = new ServiceBusClient(emulator.ConnectionString);
        await using var sender = client.CreateSender(ServiceBusEmulatorFixture.QueueName);

        for (var i = 1; i <= messageCount; i++)
        {
            await sender.SendMessageAsync(new ServiceBusMessage($"notification-{i}"));
        }

        var processed = new ConcurrentBag<(string Consumer, string Body)>();

        await using var consumer1 = StartConsumer(client, "consumer-01", processed);
        await using var consumer2 = StartConsumer(client, "consumer-02", processed);

        await consumer1.StartProcessingAsync();
        await consumer2.StartProcessingAsync();

        var deadline = DateTime.UtcNow.AddMinutes(2);
        while (processed.Count < messageCount && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100);
        }

        await consumer1.StopProcessingAsync();
        await consumer2.StopProcessingAsync();

        Assert.Equal(messageCount, processed.Select(p => p.Body).Distinct().Count());
        Assert.Contains(processed, p => p.Consumer == "consumer-01");
        Assert.Contains(processed, p => p.Consumer == "consumer-02");
    }

    private static ServiceBusProcessor StartConsumer(
        ServiceBusClient client,
        string id,
        ConcurrentBag<(string Consumer, string Body)> processed)
    {
        var processor = client.CreateProcessor(ServiceBusEmulatorFixture.QueueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        processor.ProcessMessageAsync += async args =>
        {
            processed.Add((id, args.Message.Body.ToString()));
            await args.CompleteMessageAsync(args.Message);
        };
        processor.ProcessErrorAsync += _ => Task.CompletedTask;

        return processor;
    }
}
