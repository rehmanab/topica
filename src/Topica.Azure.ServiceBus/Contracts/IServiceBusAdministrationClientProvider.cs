using Azure.Messaging.ServiceBus.Administration;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IServiceBusAdministrationClientProvider
{
    string ConnectionString { get; }
    ServiceBusAdministrationClient AdminClient { get; }
}