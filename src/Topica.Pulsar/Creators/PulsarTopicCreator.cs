using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Creators
{
    public class PulsarTopicCreator : ITopicCreator
    {
        private readonly IPulsarService _pulsarService;
        private readonly IConsumer _consumer;
        private readonly ILogger<PulsarTopicCreator> _logger;

        public PulsarTopicCreator(IPulsarService pulsarService, IConsumer consumer, ILogger<PulsarTopicCreator> logger)
        {
            _pulsarService = pulsarService;
            _consumer = consumer;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.Pulsar;
        
        public async Task<IConsumer> CreateTopic(ConsumerSettings settings)
        {
            await _pulsarService.CreateNamespaceAsync(settings.PulsarTenant, settings.PulsarNamespace);
            await _pulsarService.CreateTopicAsync(settings.PulsarTenant, settings.PulsarNamespace, settings.Source);
            
            _logger.LogInformation($"{nameof(PulsarTopicCreator)}.{nameof(CreateTopic)}: Created topic {settings.Source}");

            return _consumer;
        }
    }
}