using Azure.Messaging.ServiceBus;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IServiceBusClientProvider
{
    ServiceBusClient GetServiceBusClient();
}