namespace StorageQueue.EmulatorSample.Services;

using Azure.Storage.Queues;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Shared.EmulatorSample.Models;

/// <summary>
/// Service for sending messages to Azure Storage Queue
/// </summary>
public interface IQueueMessageSender
{
    Task SendOrderAsync(Order order);
}

/// <summary>
/// Implementation of queue message sender
/// </summary>
public class QueueMessageSender : IQueueMessageSender
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly ILogger<QueueMessageSender> _logger;
    private const string QueueName = "order-processing-queue";

    public QueueMessageSender(QueueServiceClient queueServiceClient, ILogger<QueueMessageSender> logger)
    {
        _queueServiceClient = queueServiceClient;
        _logger = logger;
    }

    public async Task SendOrderAsync(Order order)
    {
        try
        {
            var queueClient = _queueServiceClient.GetQueueClient(QueueName);
            
            // Ensure queue exists
            await queueClient.CreateIfNotExistsAsync();
            
            // Serialize order to JSON
            var orderJson = JsonSerializer.Serialize(order, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            // Send message to queue
            await queueClient.SendMessageAsync(orderJson);
            
            _logger.LogInformation("Successfully sent order {OrderId} to queue {QueueName}", 
                order.Id, QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order {OrderId} to queue", order.Id);
            throw;
        }
    }
}
