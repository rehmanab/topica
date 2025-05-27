using Azure.Messaging.ServiceBus;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IServiceBusClientProvider
{
    string ConnectionString { get; }
    ServiceBusClient Client { get; }
}