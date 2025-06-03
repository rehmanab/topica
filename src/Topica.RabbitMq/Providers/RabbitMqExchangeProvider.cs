using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Topica.Contracts;
using Topica.RabbitMq.Consumers;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Models;
using Topica.RabbitMq.Producers;
using Topica.RabbitMq.Requests;
using Topica.Settings;

namespace Topica.RabbitMq.Providers
{
    public class RabbitMqExchangeProvider(IRabbitMqManagementApiClient managementApiClient, ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<RabbitMqExchangeProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task CreateTopicAsync(MessagingSettings settings)
        {
            var queues = settings.RabbitMqWithSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true, RoutingKey = $"{subscribedQueue}_routing_key"
            }).ToList();

            await managementApiClient.CreateExchangeAndBindingsAsync(settings.Source, true, ExchangeTypes.Fanout, queues);
            
            logger.LogInformation("{RabbitMqExchangeProviderName}.{CreateTopicAsyncName}: Created exchange {Source}", nameof(RabbitMqExchangeProvider), nameof(CreateTopicAsync), settings.Source);
        }

        public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;

            return new RabbitMqQueueConsumer(rabbitMqConnectionFactory, messageHandlerExecutor, messagingSettings, logger);
        }

        public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;
            
            return new RabbitMqTopicProducer(rabbitMqConnectionFactory);
        }

        private  async Task<object> CreateTopicAsync(string source, string[] withSubscribedQueues)
        {
            var queues = withSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true, RoutingKey = $"{subscribedQueue}_routing_key"
            }).ToList();

            await managementApiClient.CreateExchangeAndBindingsAsync(source, true, ExchangeTypes.Fanout, queues);
            
            logger.LogInformation("{RabbitMqExchangeProviderName}.{CreateTopicAsyncName}: Created exchange {Source}", nameof(RabbitMqExchangeProvider), nameof(CreateTopicAsync), source);
            
            return new object();
        }
    }
}