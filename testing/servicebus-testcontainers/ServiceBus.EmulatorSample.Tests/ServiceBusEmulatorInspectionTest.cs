using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Xunit;
using ServiceBus.EmulatorSample;
using Shared.EmulatorSample.Models;
using Shared.EmulatorSample.Builders;


namespace ServiceBus.EmulatorSample.Tests;

public class ServiceBusEmulatorInspectionTest : IAsyncDisposable
{
    private const string QueueName = "order-processing-queue";
    private ServiceBusTestContainer? _containerManager;

    [Fact]
    public async Task SendAndVerifyMessages_ShouldContainExpectedMessages()
    {
        // Start the Service Bus Emulator
        _containerManager = new ServiceBusTestContainer();
        await _containerManager.StartAsync();
        
        var connectionString = _containerManager.ConnectionString;
        
        // Send test messages
        var sentOrders = await SendTestMessages(connectionString);
        
        // Verify all messages are in the queue
        await VerifyMessagesInQueue(connectionString, sentOrders);
    }
    
    private static async Task<List<Order>> SendTestMessages(string connectionString)
    {
        var sentOrders = new List<Order>();
        
        for (int i = 1; i <= 3; i++)
        {
            var testOrder = OrderBuilder.Create()
                .WithCustomer($"Customer {i}")
                .AddLaptop()
                .Build();
                
            sentOrders.Add(testOrder);
                
            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(QueueName);
            
            var orderJson = JsonSerializer.Serialize(testOrder, new JsonSerializerOptions { WriteIndented = true });
            var message = new ServiceBusMessage(orderJson)
            {
                MessageId = testOrder.Id.ToString(),
                ContentType = "application/json",
                Subject = $"Order from {testOrder.CustomerName}"
            };

            await sender.SendMessageAsync(message);
        }
        
        return sentOrders;
    }
    
    private static async Task VerifyMessagesInQueue(string connectionString, List<Order> expectedOrders)
    {
        await using var client = new ServiceBusClient(connectionString);
        await using var receiver = client.CreateReceiver(QueueName, new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock // Don't actually consume the messages
        });
        
        // Peek messages (doesn't remove them from queue)
        var messages = await receiver.PeekMessagesAsync(maxMessages: 10);
        
        // Assert we have the expected number of messages
        Assert.Equal(expectedOrders.Count, messages.Count);
        
        // Verify each message content
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            var messageBody = message.Body.ToString();
            var receivedOrder = JsonSerializer.Deserialize<Order>(messageBody);
            
            // Assert message properties
            Assert.Equal("application/json", message.ContentType);
            Assert.NotNull(message.MessageId);
            Assert.NotNull(message.Subject);
            Assert.Contains("Order from", message.Subject);
            
            // Assert order content
            Assert.NotNull(receivedOrder);
            Assert.True(expectedOrders.Any(o => o.Id == receivedOrder.Id), 
                $"Order with ID {receivedOrder.Id} was not in the expected orders");
            Assert.Contains(receivedOrder.CustomerName, expectedOrders.Select(o => o.CustomerName));
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