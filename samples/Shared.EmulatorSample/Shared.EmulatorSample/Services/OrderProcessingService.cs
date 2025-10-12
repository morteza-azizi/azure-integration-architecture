namespace Shared.EmulatorSample.Services;

using Microsoft.Extensions.Logging;
using Shared.EmulatorSample.Models;

/// <summary>
/// Service for processing orders
/// </summary>
public interface IOrderProcessingService
{
    Task ProcessOrderAsync(Order order);
}

/// <summary>
/// Implementation of order processing service
/// </summary>
public class OrderProcessingService : IOrderProcessingService
{
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(ILogger<OrderProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessOrderAsync(Order order)
    {
        _logger.LogInformation("Processing order {OrderId} for customer {CustomerName}", 
            order.Id, order.CustomerName);

        try
        {
            order.Status = OrderStatus.Processing;
            
            if (order.Items.Count == 0)
            {
                throw new InvalidOperationException("Order must have at least one item");
            }

            if (order.TotalAmount <= 0)
            {
                throw new InvalidOperationException("Order total must be greater than zero");
            }
            
            order.Status = OrderStatus.Completed;
            
            _logger.LogInformation("Successfully processed order {OrderId} with total {TotalAmount:C}", 
                order.Id, order.TotalAmount);
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}

