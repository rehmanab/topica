using Azure.Messaging.ServiceBus;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusClientProvider
{
    string DomainUrlSuffix { get; }
    string ConnectionString { get; }
    ServiceBusClient Client { get; }
}