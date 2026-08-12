using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Testcontainers.ServiceBus;

namespace ServiceBus.EmulatorSample.Tests;

/// <summary>
/// Manages the Service Bus Emulator container lifecycle and configuration
/// </summary>
public class ServiceBusTestContainer : IAsyncDisposable
{
    private const ushort ServiceBusPort = 5672;
    private const ushort ServiceBusHttpPort = 5300;
    
    private ServiceBusContainer? _serviceBusContainer;
    
    public string ConnectionString => _serviceBusContainer?.GetConnectionString() ?? 
        throw new InvalidOperationException("Container not started");
    
    public async Task StartAsync()
    {
        _serviceBusContainer = new ServiceBusBuilder()
            .WithImage("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .WithPortBinding(ServiceBusPort, true)
            .WithPortBinding(ServiceBusHttpPort, true)  // HTTP port for health checks
            .WithEnvironment("SQL_WAIT_INTERVAL", "0")  // Critical for startup timing
            .WithResourceMapping("Config.json", "/ServiceBus_Emulator/ConfigFiles/")  // Pre-configure queue
            // No wait strategy - just use time-based wait to avoid noisy logs
            .Build();
        
        await _serviceBusContainer.StartAsync();
        
        // Additional wait for emulator to be fully ready (no port checking to avoid logs)
        Console.WriteLine("Waiting for Service Bus Emulator to start...");
        await Task.Delay(TimeSpan.FromSeconds(20));
        Console.WriteLine("Service Bus Emulator should be ready");
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusContainer != null)
        {
            await _serviceBusContainer.DisposeAsync();
        }
    }
} 