using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Providers
{
    public class PulsarTopicProvider : ITopicProvider
    {
        private readonly IPulsarService _pulsarService;
        private readonly ILogger<PulsarTopicProvider> _logger;

        public PulsarTopicProvider(IPulsarService pulsarService, ILogger<PulsarTopicProvider> logger)
        {
            _pulsarService = pulsarService;
            _logger = logger;
        }
        
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
            await _pulsarService.CreateNamespaceAsync(tenant, @namespace);
            await _pulsarService.CreateTopicAsync(tenant, @namespace, source);
            
            _logger.LogInformation($"{nameof(PulsarTopicProvider)}.{nameof(CreateTopicAsync)}: Created topic {source}");
        }
    }
}