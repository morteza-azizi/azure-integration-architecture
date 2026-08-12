using Testcontainers.Azurite;

namespace StorageQueue.EmulatorSample.Tests;

/// <summary>
/// Manages the Azurite container lifecycle and configuration for Storage Queue testing
/// </summary>
public class StorageQueueTestContainer : IAsyncDisposable
{
    private const ushort BlobPort = 10000;
    private const ushort QueuePort = 10001;
    private const ushort TablePort = 10002;
    
    private AzuriteContainer? _azuriteContainer;
    
    public string ConnectionString => _azuriteContainer?.GetConnectionString() ?? 
        throw new InvalidOperationException("Container not started");
    
    public async Task StartAsync()
    {
        _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.36.0")
            .WithPortBinding(BlobPort, true)   // Blob service
            .WithPortBinding(QueuePort, true)  // Queue service
            .WithPortBinding(TablePort, true)  // Table service
            .Build();
        
        await _azuriteContainer.StartAsync();
        
        // Wait for Azurite to be fully ready
        Console.WriteLine("Waiting for Azurite to start...");
        await Task.Delay(TimeSpan.FromSeconds(5));
        Console.WriteLine("Azurite should be ready");
    }

    public async ValueTask DisposeAsync()
    {
        if (_azuriteContainer != null)
        {
            await _azuriteContainer.DisposeAsync();
        }
    }
}
