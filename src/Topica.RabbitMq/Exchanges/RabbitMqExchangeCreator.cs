using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Clients;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using Topica.RabbitMq.Settings;
using Topica.Topics;

namespace Topica.RabbitMq.Exchanges
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

        public async Task<IConsumer> CreateTopic(TopicSettingsBase settings)
        {
            var config = settings as RabbitMqTopicSettings;

            if (config == null)
            {
                throw new Exception($"{nameof(RabbitMqExchangeCreator)}.{nameof(CreateTopic)} needs an {nameof(RabbitMqExchangeCreator)} ");
            }

            var queues = config.WithSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true
            }).ToList();

            await _managementApiClient.CreateAsync(config.TopicName, true, ExchangeTypes.Fanout, queues);
            
            _logger.LogInformation($"{nameof(RabbitMqExchangeCreator)}.{nameof(CreateTopic)}: Created exchange {config.TopicName}");
            
            return _consumer;
        }
    }
}