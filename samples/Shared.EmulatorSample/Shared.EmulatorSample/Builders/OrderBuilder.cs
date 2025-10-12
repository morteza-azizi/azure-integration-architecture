namespace Shared.EmulatorSample.Builders;

using Shared.EmulatorSample.Models;

/// <summary>
/// Immutable builder pattern for creating test Order objects
/// Each method returns a new instance, ensuring thread-safety and reusability
/// </summary>
public class OrderBuilder
{
    private readonly Guid _id;
    private readonly string _customerName;
    private readonly string _customerEmail;
    private readonly List<OrderItem> _items;
    private readonly OrderStatus _status;

    private OrderBuilder(
        Guid? id = null,
        string? customerName = null,
        string? customerEmail = null,
        List<OrderItem>? items = null,
        OrderStatus status = OrderStatus.Pending)
    {
        _id = id ?? Guid.NewGuid();
        _customerName = customerName ?? string.Empty;
        _customerEmail = customerEmail ?? string.Empty;
        _items = items ?? new List<OrderItem>();
        _status = status;
    }

    public static OrderBuilder Create() => new();

    public OrderBuilder WithDefaults()
    {
        return new OrderBuilder(
            _id,
            "Test Customer",
            "test@example.com",
            _items,
            OrderStatus.Pending);
    }

    public OrderBuilder WithCustomer(string customerName)
    {
        var email = $"{customerName.ToLower().Replace(" ", ".")}@example.com";
        return new OrderBuilder(_id, customerName, email, _items, _status);
    }

    public OrderBuilder WithEmail(string email)
    {
        return new OrderBuilder(_id, _customerName, email, _items, _status);
    }

    public OrderBuilder AddLaptop()
    {
        var laptop = new OrderItem
        {
            ProductName = "Gaming Laptop",
            Quantity = 1,
            UnitPrice = 1299.99m
        };
        
        var newItems = new List<OrderItem>(_items) { laptop };
        return new OrderBuilder(_id, _customerName, _customerEmail, newItems, _status);
    }

    public OrderBuilder AddMouse()
    {
        var mouse = new OrderItem
        {
            ProductName = "Wireless Mouse",
            Quantity = 1,
            UnitPrice = 29.99m
        };
        
        var newItems = new List<OrderItem>(_items) { mouse };
        return new OrderBuilder(_id, _customerName, _customerEmail, newItems, _status);
    }

    public OrderBuilder AddKeyboard()
    {
        var keyboard = new OrderItem
        {
            ProductName = "Mechanical Keyboard",
            Quantity = 1,
            UnitPrice = 149.99m
        };
        
        var newItems = new List<OrderItem>(_items) { keyboard };
        return new OrderBuilder(_id, _customerName, _customerEmail, newItems, _status);
    }

    public OrderBuilder WithStatus(OrderStatus status)
    {
        return new OrderBuilder(_id, _customerName, _customerEmail, _items, status);
    }

    public OrderBuilder WithId(Guid id)
    {
        return new OrderBuilder(id, _customerName, _customerEmail, _items, _status);
    }

    public Order Build()
    {
        return new Order
        {
            Id = _id,
            CustomerName = _customerName,
            CustomerEmail = _customerEmail,
            Items = _items.ToList(), // Create new list
            TotalAmount = _items.Sum(item => item.TotalPrice),
            CreatedAt = DateTime.UtcNow,
            Status = _status
        };
    }
}

