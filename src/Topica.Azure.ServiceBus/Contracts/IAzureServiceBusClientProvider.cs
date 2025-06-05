using Azure.Messaging.ServiceBus;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusClientProvider
{
    string ConnectionString { get; }
    ServiceBusClient Client { get; }
}