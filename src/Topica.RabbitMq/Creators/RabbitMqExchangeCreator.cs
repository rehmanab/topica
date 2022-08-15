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
    public class RabbitMqExchangeCreator : ITopicCreator
    {
        private readonly IRabbitMqManagementApiClient _managementApiClient;
        private readonly IConsumer _consumer;
        private readonly ILogger<RabbitMqExchangeCreator> _logger;

        public RabbitMqExchangeCreator(IRabbitMqManagementApiClient managementApiClient, IConsumer consumer, ILogger<RabbitMqExchangeCreator> logger)
        {
            _managementApiClient = managementApiClient;
            _consumer = consumer;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task<IConsumer> CreateTopic(ConsumerSettings settings)
        {
            var queues = settings.WithSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true
            }).ToList();

            await _managementApiClient.CreateAsync(settings.Source, true, ExchangeTypes.Fanout, queues);
            
            _logger.LogInformation($"{nameof(RabbitMqExchangeCreator)}.{nameof(CreateTopic)}: Created exchange {settings.Source}");
            
            return _consumer;
        }
    }
}