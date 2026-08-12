using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.EmulatorSample.Models;

namespace ServiceBus.EmulatorSample;

public class MessageSender : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<MessageSender> _logger;

    public MessageSender(ILogger<MessageSender> logger)
    {
        _logger = logger;
        
        // Always use managed identity for authentication
        var credential = new DefaultAzureCredential();
        var serviceBusNamespace = Environment.GetEnvironmentVariable("SERVICE_BUS_FULLY_QUALIFIED_NAMESPACE") ?? "localhost";
        var queueName = Environment.GetEnvironmentVariable("QueueName") ?? "order-processing-queue";
        
        _client = new ServiceBusClient(serviceBusNamespace, credential);
        _sender = _client.CreateSender(queueName);
    }

    public async Task SendOrderAsync(Order order)
    {
        var orderBytes = JsonSerializer.SerializeToUtf8Bytes(order);

        var message = new ServiceBusMessage(orderBytes)
        {
            MessageId = order.Id.ToString(),
            ContentType = "application/json",
            Subject = "OrderCreated"
        };

        await _sender.SendMessageAsync(message);
        _logger.LogInformation("Order sent: {OrderId} for customer {CustomerName} with {ItemCount} items", 
            order.Id, order.CustomerName, order.Items.Count);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
} 