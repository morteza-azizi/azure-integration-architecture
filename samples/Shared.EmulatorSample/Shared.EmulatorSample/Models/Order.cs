namespace Shared.EmulatorSample.Models;

/// <summary>
/// Represents an order in the system
/// </summary>
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// Order processing status
/// </summary>
public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

