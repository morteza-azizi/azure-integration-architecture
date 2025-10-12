using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Queues;
using Shared.EmulatorSample.Services;
using StorageQueue.EmulatorSample.Services;

namespace StorageQueue.EmulatorSample;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                // Register Azure Storage Queue client
                services.AddSingleton<QueueServiceClient>(provider =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") 
                        ?? "UseDevelopmentStorage=true";
                    return new QueueServiceClient(connectionString);
                });

                // Register services
                services.AddScoped<IOrderProcessingService, OrderProcessingService>();
                services.AddScoped<IQueueMessageSender, QueueMessageSender>();
            })
            .Build();

        host.Run();
    }
}
