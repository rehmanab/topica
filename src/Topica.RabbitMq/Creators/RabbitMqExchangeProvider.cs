using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using Topica.Settings;

namespace Topica.RabbitMq.Creators
{
    public class RabbitMqExchangeProvider : ITopicProvider
    {
        private readonly IRabbitMqManagementApiClient _managementApiClient;
        private readonly IConsumer _consumer;
        private readonly IProducerBuilder _producerBuilder;
        private readonly ILogger<RabbitMqExchangeProvider> _logger;

        public RabbitMqExchangeProvider(IRabbitMqManagementApiClient managementApiClient, IConsumer consumer, IProducerBuilder producerBuilder, ILogger<RabbitMqExchangeProvider> logger)
        {
            _managementApiClient = managementApiClient;
            _consumer = consumer;
            _producerBuilder = producerBuilder;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task<IConsumer> CreateTopicAsync(ConsumerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues);
            
            return _consumer;
        }

        public async Task<IProducerBuilder> CreateTopicAsync(ProducerSettings settings)
        {
            await CreateTopicAsync(settings.Source, settings.WithSubscribedQueues);
            
            return _producerBuilder;
        }

        public IConsumer GetConsumer()
        {
            throw new System.NotImplementedException();
        }

        public IProducerBuilder GetProducerBuilder()
        {
            throw new System.NotImplementedException();
        }

        private  async Task<IConsumer> CreateTopicAsync(string source, string[] withSubscribedQueues)
        {
            var queues = withSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true
            }).ToList();

            await _managementApiClient.CreateAsync(source, true, ExchangeTypes.Fanout, queues);
            
            _logger.LogInformation($"{nameof(RabbitMqExchangeProvider)}.{nameof(CreateTopicAsync)}: Created exchange {source}");
            
            return _consumer;
        }
    }
}