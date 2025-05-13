using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Providers
{
    public class PulsarTopicProvider(IPulsarService pulsarService, ILogger<PulsarTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Pulsar;
        
        public async Task CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source);
        }

        public async Task CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source);
        }

        private async Task CreateTopicAsync(string tenant, string @namespace, string source)
        {
            await pulsarService.CreateNamespaceAsync(tenant, @namespace);
            await pulsarService.CreateTopicAsync(tenant, @namespace, source);
            
            logger.LogInformation("{PulsarTopicProviderName}.{CreateTopicAsyncName}: Created topic {Source}", nameof(PulsarTopicProvider), nameof(CreateTopicAsync), source);
        }
    }
}