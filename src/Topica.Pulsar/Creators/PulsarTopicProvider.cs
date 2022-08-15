using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Creators
{
    public class PulsarTopicProvider : ITopicProvider
    {
        private readonly IPulsarService _pulsarService;
        private readonly IConsumer _consumer;
        private readonly IProducerBuilder _producerBuilder;
        private readonly ILogger<PulsarTopicProvider> _logger;

        public PulsarTopicProvider(IPulsarService pulsarService, IConsumer consumer, IProducerBuilder producerBuilder, ILogger<PulsarTopicProvider> logger)
        {
            _pulsarService = pulsarService;
            _consumer = consumer;
            _producerBuilder = producerBuilder;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Pulsar;
        
        public async Task<IConsumer> CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source);

            return _consumer;
        }

        public async Task<IProducerBuilder> CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source);

            return _producerBuilder;
        }

        public IConsumer GetConsumer()
        {
            return _consumer;
        }

        public IProducerBuilder GetProducerBuilder()
        {
            return _producerBuilder;
        }

        private async Task CreateTopicAsync(string tenant, string @namespace, string source)
        {
            await _pulsarService.CreateNamespaceAsync(tenant, @namespace);
            await _pulsarService.CreateTopicAsync(tenant, @namespace, source);
            
            _logger.LogInformation($"{nameof(PulsarTopicProvider)}.{nameof(CreateTopicAsync)}: Created topic {source}");
        }
    }
}