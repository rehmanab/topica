using Azure.Messaging.ServiceBus.Administration;
using Topica.Azure.ServiceBus.Contracts;

namespace Topica.Azure.ServiceBus.Providers;

public class ServiceBusAdministrationClientProvider : IServiceBusAdministrationClientProvider
{
    public ServiceBusAdministrationClientProvider(string connectionString, ServiceBusAdministrationClientOptions? options = null)
    {
        ConnectionString = connectionString;
        AdminClient = options == null 
            ? new ServiceBusAdministrationClient(ConnectionString)
            : new ServiceBusAdministrationClient(ConnectionString, options);
    }

    public string ConnectionString { get; }
    public ServiceBusAdministrationClient AdminClient { get; }
}