using System.Text.Json;
using Azure.Storage.Queues;
using Xunit;
using StorageQueue.EmulatorSample;
using Shared.EmulatorSample.Models;
using Shared.EmulatorSample.Builders;

namespace StorageQueue.EmulatorSample.Tests;

public class StorageQueueInspectionTest : IAsyncDisposable
{
    private const string QueueName = "order-processing-queue";
    private StorageQueueTestContainer? _containerManager;

    [Fact]
    public async Task SendAndVerifyMessages_ShouldContainExpectedMessages()
    {
        Console.WriteLine("[TEST] Starting Azurite container...");
        
        // Start the Azurite container
        _containerManager = new StorageQueueTestContainer();
        await _containerManager.StartAsync();
        
        var connectionString = _containerManager.ConnectionString;
        Console.WriteLine($"[TEST] Azurite started. Connection: {connectionString}");
        
        // Send test messages
        Console.WriteLine("[TEST] Sending 3 test messages...");
        var sentOrders = await SendTestMessages(connectionString);
        Console.WriteLine($"[TEST] Sent {sentOrders.Count} messages to queue '{QueueName}'");
        
        // Verify all messages are in the queue
        Console.WriteLine("[TEST] Verifying messages in queue...");
        await VerifyMessagesInQueue(connectionString, sentOrders);
        Console.WriteLine("[TEST] All messages verified successfully!");
    }
    
    private static async Task<List<Order>> SendTestMessages(string connectionString)
    {
        var sentOrders = new List<Order>();
        var queueServiceClient = new QueueServiceClient(connectionString);
        var queueClient = queueServiceClient.GetQueueClient(QueueName);
        
        // Ensure queue exists
        await queueClient.CreateIfNotExistsAsync();
        
        for (int i = 1; i <= 3; i++)
        {
            var testOrder = OrderBuilder.Create()
                .WithCustomer($"Customer {i}")
                .AddLaptop()
                .Build();
                
            sentOrders.Add(testOrder);
            
            var orderJson = JsonSerializer.Serialize(testOrder, new JsonSerializerOptions { WriteIndented = true });
            await queueClient.SendMessageAsync(orderJson);
            
            Console.WriteLine($"[TEST]   Sent message {i}: Order {testOrder.Id} for {testOrder.CustomerName}");
        }
        
        return sentOrders;
    }
    
    private static async Task VerifyMessagesInQueue(string connectionString, List<Order> expectedOrders)
    {
        var queueServiceClient = new QueueServiceClient(connectionString);
        var queueClient = queueServiceClient.GetQueueClient(QueueName);
        
        // Peek messages (doesn't remove them from queue)
        var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);
        
        Console.WriteLine($"[TEST]   Peeked {peekedMessages.Value.Length} messages from queue");
        
        // Assert we have the expected number of messages
        Assert.Equal(expectedOrders.Count, peekedMessages.Value.Length);
        
        // Verify each message content
        for (int i = 0; i < peekedMessages.Value.Length; i++)
        {
            var message = peekedMessages.Value[i];
            var messageBody = message.MessageText;
            var receivedOrder = JsonSerializer.Deserialize<Order>(messageBody);
            
            // Assert message properties
            Assert.NotNull(message.MessageId);
            Assert.NotNull(message.InsertedOn);
            Assert.True(message.ExpiresOn > message.InsertedOn);
            Assert.Equal(0, message.DequeueCount); // Never been dequeued
            
            // Assert order content
            Assert.NotNull(receivedOrder);
            Assert.True(expectedOrders.Any(o => o.Id == receivedOrder.Id), 
                $"Order with ID {receivedOrder.Id} was not in the expected orders");
            Assert.Contains(receivedOrder.CustomerName, expectedOrders.Select(o => o.CustomerName));
            
            Console.WriteLine($"[TEST]   Verified message {i + 1}: {receivedOrder.CustomerName} - Order {receivedOrder.Id}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_containerManager != null)
        {
            await _containerManager.DisposeAsync();
        }
    }
}
