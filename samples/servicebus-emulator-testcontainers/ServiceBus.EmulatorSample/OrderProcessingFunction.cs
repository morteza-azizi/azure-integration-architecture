using Microsoft.Azure.Functions.Worker;
using Shared.EmulatorSample.Models;
using Shared.EmulatorSample.Services;

namespace ServiceBus.EmulatorSample;

public class OrderProcessingFunction(IOrderProcessingService orderProcessingService)
{
    [Function("OrderProcessingFunction")]
    public async Task Run(
        [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnection")] Order order)
    {
        await orderProcessingService.ProcessOrderAsync(order);
    }
}
