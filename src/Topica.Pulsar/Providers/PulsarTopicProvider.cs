using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pulsar.Client.Api;
using Topica.Contracts;
using Topica.Pulsar.Consumers;
using Topica.Pulsar.Contracts;
using Topica.Pulsar.Producers;
using Topica.Settings;

namespace Topica.Pulsar.Providers
{
    public class PulsarTopicProvider(IPulsarService pulsarService, PulsarClientBuilder pulsarClientBuilder, IMessageHandlerExecutor messageHandlerExecutor, ILogger<PulsarTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Pulsar;
        
        public async Task CreateTopicAsync(MessagingSettings settings)
        {
            await pulsarService.CreateTenantAsync(settings.PulsarTenant);
            await pulsarService.CreateNamespaceAsync(settings.PulsarTenant, settings.PulsarNamespace);
            await pulsarService.CreatePartitionedTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source, settings.PulsarTopicNumberOfPartitions);
            
            logger.LogInformation("**** CREATED: {PulsarTopicProviderName}.{CreateTopicAsyncName}: topic {Source}", nameof(PulsarTopicProvider), nameof(CreateTopicAsync), settings.Source);
        }

        public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;

            return new PulsarTopicConsumer(pulsarClientBuilder, messageHandlerExecutor, messagingSettings, logger);
        }

        public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;
            
            return new PulsarTopicProducer(producerName, pulsarClientBuilder, messagingSettings);
        }
    }
}