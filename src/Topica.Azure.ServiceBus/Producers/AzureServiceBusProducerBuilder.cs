using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Helpers;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Producers;

public class AzureServiceBusProducerBuilder(ITopicProviderFactory topicProviderFactory, IServiceBusClientProvider provider, ILogger<AzureServiceBusProducerBuilder> logger) : IProducerBuilder
{
    public async Task<T> BuildProducerAsync<T>(string producerName, ProducerSettings producerSettings, CancellationToken cancellationToken)
    {
        var connectionStringEndpoint = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(producerSettings.ConnectionString!);
        if (!string.IsNullOrWhiteSpace(connectionStringEndpoint) && connectionStringEndpoint.Contains(".servicebus.windows.net"))
        {
            logger.LogInformation("Azure Service Bus Producer: {ProducerName} with connection string endpoint: {ConnectionStringEndpoint}", producerName, connectionStringEndpoint);
            await topicProviderFactory.Create(MessagingPlatform.AzureServiceBus).CreateTopicAsync(producerSettings);
        }
        else
        {
            logger.LogInformation("Azure Service Bus Producer: {ProducerName} is using the Emulator endpoint: {ConnectionStringEndpoint} .. Skipping Creation as it's not supported", producerName, connectionStringEndpoint);
        }

        return (T)provider;
    }
}