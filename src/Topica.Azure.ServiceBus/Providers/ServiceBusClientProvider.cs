using Azure.Messaging.ServiceBus;
using Topica.Azure.ServiceBus.Contracts;

namespace Topica.Azure.ServiceBus.Providers;

public class ServiceBusClientProvider(ServiceBusClient client) : IServiceBusClientProvider
{
    public ServiceBusClient GetServiceBusClient()
    {
        return client ?? throw new ArgumentNullException(nameof(client), "ServiceBusClient cannot be null.");
    }
}