# ServiceBus.EmulatorSample

A clean example of testing Azure Functions with Service Bus using containerized emulators.

## 🤔 Why This Sample?

**The Problem:** Testing Azure Functions with Service Bus is painful. You need:
- Real Azure resources (expensive, slow)
- Complex setup and teardown
- Most teams skip integration testing entirely (leading to production bugs)
- No way to reproduce Service Bus issues locally for debugging

**The Solution:** This sample shows how to:
- Test Azure Functions locally with Service Bus Emulator
- Use Testcontainers for reliable, isolated testing
- Ship to production with confidence (comprehensive integration tests)
- Debug Service Bus issues on your laptop (no more "works on my machine")

## 🚀 How to Use It

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop

### Run the Tests
```bash
# Clone and run
git clone <your-repo>
cd ServiceBus.EmulatorSample
dotnet test
```

That's it! The tests will:
1. Start Service Bus Emulator in a container
2. Send test messages
3. Verify everything works
4. Clean up automatically

### Run the Function Locally
```bash
# Build and run
dotnet build
cd ServiceBus.EmulatorSample
func start
```

### Project Structure
```
ServiceBus.EmulatorSample/
├── OrderProcessingFunction.cs          # Azure Function with Service Bus trigger
├── Models/Order.cs                     # Business model
├── Services/OrderProcessingService.cs  # Business logic
└── Services/MessageSender.cs           # Message sending helper

ServiceBus.EmulatorSample.Tests/
├── ServiceBusEndToEndTest.cs          # Integration tests
├── ServiceBusEmulatorInspectionTest.cs # Message inspection tests
├── ServiceBusTestContainer.cs         # Container management
└── Config.json                        # Emulator configuration
```

## ✅ What You Achieve

After running this sample, you'll know how to:

### 🎯 **Test Azure Functions Properly**
- No more manual testing or expensive cloud resources
- Reliable, fast, isolated tests that run anywhere
- Proper mocking and dependency injection

### 🐳 **Use Testcontainers Effectively**
- Start/stop Service Bus Emulator programmatically
- Handle container lifecycle properly
- Configure emulator with JSON settings


### 🏗️ **Build Clean Architecture**
- Separate business logic from Azure Functions
- Use dependency injection properly
- Apply builder pattern for test data

## 🔧 Key Components

| Component | Purpose |
|-----------|---------|
| `OrderProcessingFunction` | Azure Function that processes orders from Service Bus |
| `OrderProcessingService` | Business logic separated from function trigger |
| `MessageSender` | Helper for sending messages to Service Bus |
| `ServiceBusTestContainer` | Manages Service Bus Emulator lifecycle |
| `OrderBuilder` | Test data creation with builder pattern |

## 🧪 Testing Strategy

```csharp
// Integration test - full end-to-end
[Fact]
public async Task Can_SendMessage_Successfully()
{
    var order = new OrderBuilder().WithDefaults().Build(); 
    await _messageSender.SendOrderAsync(order);
    // Verify message sent successfully
}

// Inspection test - verify message content
[Fact]
public async Task Can_InspectMessages_Programmatically()
{
    // Send message and inspect queue content
    // Assert on message properties and content
}
```

## 🚨 Important Notes

- **Service Bus Explorer doesn't work with emulators** - use the inspection tests instead
- **Emulator uses AMQP port 5672** - ensure it's available
- **Tests are self-contained** - no external dependencies

## 🎓 Learning Outcomes

By the end, you'll have a production-ready pattern for:
- Testing Azure Functions with Service Bus
- Using Testcontainers in .NET projects  
- Building testable, maintainable serverless applications
- Avoiding common pitfalls with Service Bus testing

---