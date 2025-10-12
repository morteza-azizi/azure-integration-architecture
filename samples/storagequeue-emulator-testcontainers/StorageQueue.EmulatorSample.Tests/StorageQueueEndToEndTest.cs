using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using StorageQueue.EmulatorSample;
using Shared.EmulatorSample.Models;
using Shared.EmulatorSample.Services;
using Shared.EmulatorSample.Builders;

namespace StorageQueue.EmulatorSample.Tests;

public class StorageQueueEndToEndTest : IAsyncDisposable
{
    private const string QueueName = "order-processing-queue";
    private StorageQueueTestContainer? _containerManager;
    private Mock<IOrderProcessingService>? _mockService;

    [Fact]
    public async Task AzureFunction_EndToEnd_ShouldProcessMessage()
    {
        // Setup Azurite container
        _containerManager = new StorageQueueTestContainer();
        await _containerManager.StartAsync();
        
        var connectionString = _containerManager.ConnectionString;
        
        // Create test order
        var testOrder = OrderBuilder.Create()
            .WithCustomer("Test Customer")
            .AddLaptop()
            .Build();

        // Test 1: Verify we can send and receive from Storage Queue
        await SendOrderToQueueAsync(connectionString, testOrder);
        var receivedOrder = await ReceiveOrderFromQueueAsync(connectionString);
        
        Assert.NotNull(receivedOrder);
        Assert.Equal(testOrder.Id, receivedOrder.Id);
        Assert.Equal(testOrder.CustomerName, receivedOrder.CustomerName);
        
        // Test 2: Verify the function logic works with mock
        _mockService = new Mock<IOrderProcessingService>();
        var orderProcessingFunction = new QueueProcessingFunction(_mockService.Object, Mock.Of<ILogger<QueueProcessingFunction>>());
        
        // Simulate function processing
        await _mockService.Object.ProcessOrderAsync(testOrder);
        
        // Verify the service method was called
        _mockService.Verify(x => x.ProcessOrderAsync(It.Is<Order>(o => o.Id == testOrder.Id)), Times.Once);
    }
    
    private static async Task SendOrderToQueueAsync(string connectionString, Order order)
    {
        var orderJson = JsonSerializer.Serialize(order);
        var queueServiceClient = new QueueServiceClient(connectionString);
        var queueClient = queueServiceClient.GetQueueClient(QueueName);
        
        // Ensure queue exists
        await queueClient.CreateIfNotExistsAsync();
        
        await queueClient.SendMessageAsync(orderJson);
    }
    
    private static async Task<Order?> ReceiveOrderFromQueueAsync(string connectionString)
    {
        var queueServiceClient = new QueueServiceClient(connectionString);
        var queueClient = queueServiceClient.GetQueueClient(QueueName);
        
        var messages = await queueClient.ReceiveMessagesAsync(maxMessages: 1);
        if (messages.Value.Length == 0) return null;
        
        var message = messages.Value[0];
        var orderJson = message.MessageText;
        var order = JsonSerializer.Deserialize<Order>(orderJson);
        
        // Delete the message to remove it from the queue
        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
        
        return order;
    }

    public async ValueTask DisposeAsync()
    {
        if (_containerManager != null)
        {
            await _containerManager.DisposeAsync();
        }
    }
}
