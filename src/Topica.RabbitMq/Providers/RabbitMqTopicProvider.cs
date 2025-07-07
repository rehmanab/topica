using System.Linq;
using System.Threading;
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
    public class RabbitMqTopicProvider(IRabbitMqManagementApiClient managementApiClient, ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<RabbitMqTopicProvider> logger) : ITopicProvider
    {
        public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

        public async Task CreateTopicAsync(MessagingSettings settings, CancellationToken cancellationToken)
        {
            var queues = settings.RabbitMqWithSubscribedQueues.Select(subscribedQueue => new CreateRabbitMqQueueRequest
            {
                Name = subscribedQueue, Durable = true, RoutingKey = $"{subscribedQueue}_routing_key"
            }).ToList();

            await managementApiClient.CreateVHostIfNotExistAsync();
            await managementApiClient.CreateExchangeAndBindingsAsync(settings.Source, true, ExchangeTypes.Fanout, queues);
            
            logger.LogInformation("**** CREATE IF NOT EXIST: {RabbitMqTopicProvider}.{CreateTopicAsyncName}: Created if not already existed: exchange {Source}", nameof(RabbitMqTopicProvider), nameof(CreateTopicAsync), settings.Source);
        }

        public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;

            return new RabbitMqQueueConsumer(rabbitMqConnectionFactory, messageHandlerExecutor, messagingSettings, logger);
        }

        public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
        {
            await Task.CompletedTask;
            
            return new RabbitMqTopicProducer(producerName, rabbitMqConnectionFactory, messagingSettings);
        }
    }
}