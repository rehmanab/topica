using Azure.Messaging.ServiceBus.Administration;

namespace Topica.Azure.ServiceBus.Contracts;

public interface IAzureServiceBusAdministrationClientProvider
{
    string ConnectionString { get; }
    ServiceBusAdministrationClient AdminClient { get; }
}