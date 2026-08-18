using Testcontainers.ServiceBus;

namespace CompetingConsumers.Tests;

public sealed class ServiceBusEmulatorFixture : IAsyncDisposable
{
    private const ushort ServiceBusPort = 5672;
    private const ushort ServiceBusHttpPort = 5300;

    private ServiceBusContainer? _container;

    public string ConnectionString =>
        _container?.GetConnectionString()
        ?? throw new InvalidOperationException("Container not started.");

    public const string QueueName = "notifications";

    public async Task StartAsync()
    {
        _container = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .WithPortBinding(ServiceBusPort, true)
            .WithPortBinding(ServiceBusHttpPort, true)
            .WithEnvironment("SQL_WAIT_INTERVAL", "0")
            .WithResourceMapping("Config.json", "/ServiceBus_Emulator/ConfigFiles/")
            .Build();

        await _container.StartAsync();
        Console.WriteLine("Waiting for Service Bus Emulator…");
        await Task.Delay(TimeSpan.FromSeconds(20));
        Console.WriteLine("Service Bus Emulator ready.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
