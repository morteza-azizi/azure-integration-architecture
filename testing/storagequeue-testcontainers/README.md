# Azure Storage Queue Testing with Testcontainers

A clean example of testing Azure Functions with Storage Queue using Azurite emulator in Docker containers.

## Quick Start

### Prerequisites
- .NET 10 SDK
- Docker Desktop

### Run the Tests
```bash
dotnet test
```

That's it! The tests will automatically:
1. Start Azurite in a Docker container
2. Create queues and send test messages
3. Verify everything works
4. Clean up containers

### Run the Function Locally
```bash
cd StorageQueue.EmulatorSample
func start
```

## Project Structure

```
StorageQueue.EmulatorSample/
├── QueueProcessingFunction.cs     # Azure Function with Queue trigger
├── Services/
│   └── QueueMessageSender.cs      # Message sending helper
└── Program.cs                      # DI configuration

StorageQueue.EmulatorSample.Tests/
├── StorageQueueEndToEndTest.cs        # Integration tests
├── StorageQueueInspectionTest.cs      # Message inspection
└── StorageQueueTestContainer.cs       # Container lifecycle

Shared.EmulatorSample/                 # Shared across all samples
├── Models/Order.cs
├── Services/OrderProcessingService.cs
└── Builders/OrderBuilder.cs
```

## The Azure Function

Clean and simple:

```csharp
public class QueueProcessingFunction(IOrderProcessingService orderProcessingService, ILogger<QueueProcessingFunction> logger)
{
    [Function(nameof(QueueProcessingFunction))]
    public async Task Run([QueueTrigger("order-processing-queue")] Order order)
    {
        await orderProcessingService.ProcessOrderAsync(order);
        logger.LogInformation("Successfully processed order: {OrderId}", order.Id);
    }
}
```

## Testing Approach

### Integration Test
Verify queue send and receive operations:

```csharp
[Fact]
public async Task AzureFunction_EndToEnd_ShouldProcessMessage()
{
    var containerManager = new StorageQueueTestContainer();
    await containerManager.StartAsync();
    
    var testOrder = OrderBuilder.Create().WithCustomer("Test Customer").AddLaptop().Build();
    
    await SendOrderToQueueAsync(connectionString, testOrder);
    var receivedOrder = await ReceiveOrderFromQueueAsync(connectionString);
    
    Assert.Equal(testOrder.Id, receivedOrder.Id);
}
```

### Inspection Test
Peek messages without consuming them:

```csharp
[Fact]
public async Task SendAndVerifyMessages_ShouldContainExpectedMessages()
{
    // Start container and send 3 test messages
    var sentOrders = await SendTestMessages(connectionString);
    
    // Peek messages (doesn't remove them)
    var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);
    
    // Verify all messages are there
    Assert.Equal(3, peekedMessages.Value.Length);
}
```

## Queue Exploration

### With Azure Storage Explorer (GUI)
1. Download [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer)
2. Connect to local emulator: `UseDevelopmentStorage=true`
3. Navigate to Queues → inspect messages visually

### Programmatically (Code)
```csharp
var peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);

foreach (var message in peekedMessages.Value)
{
    Console.WriteLine($"Message ID: {message.MessageId}");
    Console.WriteLine($"Content: {message.MessageText}");
    Console.WriteLine($"Dequeue Count: {message.DequeueCount}");
}
```

## Key Benefits

✅ **No cloud resources needed** - Everything runs locally  
✅ **Fast tests** - Complete in ~15 seconds  
✅ **Isolated** - Fresh container for each test  
✅ **CI/CD ready** - Runs anywhere Docker is available  
✅ **Easy debugging** - Azure Storage Explorer works perfectly  

## Learn More

Read about the journey of building this sample: [Azure Storage Queue Testing Journey](azure-storage-queue-testing-journey.md)

---
