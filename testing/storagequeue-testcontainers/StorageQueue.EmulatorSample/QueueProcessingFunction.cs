using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.EmulatorSample.Models;
using Shared.EmulatorSample.Services;

namespace StorageQueue.EmulatorSample;

public class QueueProcessingFunction(IOrderProcessingService orderProcessingService, ILogger<QueueProcessingFunction> logger)
{
    [Function(nameof(QueueProcessingFunction))]
    public async Task Run([QueueTrigger("order-processing-queue", Connection = "AzureWebJobsStorage")] Order order)
    {
            await orderProcessingService.ProcessOrderAsync(order);
            
            logger.LogInformation("✅ Successfully processed order: {OrderId}", order.Id);
    }
}
