using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Configuration;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Requests;
using Topica.RabbitMq.Services;
using Topica.Topics;

namespace Topica.RabbitMq.Exchanges
{
    public class RabbitMqExchangeCreator : ITopicCreator
    {
        private readonly IRabbitMqManagementService _managementService;
        private readonly IConsumer _consumer;
        private readonly ILogger<RabbitMqExchangeCreator> _logger;

        public RabbitMqExchangeCreator(IRabbitMqManagementService managementService, IConsumer consumer, ILogger<RabbitMqExchangeCreator> logger)
        {
            _managementService = managementService;
            _consumer = consumer;
            _logger = logger;
        }
        
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task<IConsumer> CreateTopic(TopicConfigurationBase configuration)
        {
            var config = configuration as RabbitMqTopicConfiguration;

            if (config == null)
            {
                throw new Exception($"{nameof(RabbitMqExchangeCreator)}.{nameof(CreateTopic)} needs an {nameof(RabbitMqExchangeCreator)} ");
            }

            var queues = config.WithSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true
            }).ToList();

            await _managementService.CreateAsync(config.TopicName, true, ExchangeTypes.Fanout, queues);
            
            _logger.LogInformation($"{nameof(RabbitMqExchangeCreator)}.{nameof(CreateTopic)}: Created exchange {config.TopicName}");
            
            return _consumer;
        }
    }
}