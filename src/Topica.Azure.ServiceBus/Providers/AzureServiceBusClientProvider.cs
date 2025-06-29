using Azure.Messaging.ServiceBus;
using Topica.Azure.ServiceBus.Contracts;

namespace Topica.Azure.ServiceBus.Providers;

public class AzureServiceBusClientProvider : IAzureServiceBusClientProvider
{
    public AzureServiceBusClientProvider(string connectionString, ServiceBusClientOptions? options = null)
    {
        ConnectionString = connectionString;
        Client = options == null 
            ? new ServiceBusClient(ConnectionString)
            : new ServiceBusClient(ConnectionString, options);
    }

    public string DomainUrlSuffix => ".servicebus.windows.net";
    public string ConnectionString { get; }
    public ServiceBusClient Client { get; }
}