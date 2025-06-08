using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Topica.Contracts;
using Topica.RabbitMq.Consumers;
using Topica.RabbitMq.Contracts;
using Topica.RabbitMq.Producers;
using Topica.Settings;

namespace Topica.RabbitMq.Providers;

public class RabbitMqQueueProvider(IRabbitMqManagementApiClient managementApiClient, ConnectionFactory rabbitMqConnectionFactory, IMessageHandlerExecutor messageHandlerExecutor, ILogger<RabbitMqQueueProvider> logger) : IQueueProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.RabbitMq;

    public async Task CreateQueueAsync(MessagingSettings settings)
    {
        await managementApiClient.CreateVHostIfNotExistAsync();
        await managementApiClient.CreateQueueAsync(settings.Source, true);
            
        logger.LogInformation("**** CREATE IF NOT EXIST: {RabbitMqQueueProvider}.{CreateQueueAsyncName}: Created if not already existed: queue {Source}", nameof(RabbitMqQueueProvider), nameof(CreateQueueAsync), settings.Source);
    }

    public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new RabbitMqQueueConsumer(rabbitMqConnectionFactory, messageHandlerExecutor, messagingSettings, logger);
    }

    public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new RabbitMqQueueProducer(rabbitMqConnectionFactory);
    }
}