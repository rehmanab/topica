using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqQueueCreationBuilder(IQueueProviderFactory queueProviderFactory, ILogger<RabbitMqTopicCreationBuilder> logger) : IRabbitMqQueueCreationBuilder, IRabbitMqQueueBuilderWithQueueName, IRabbitMqQueueBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;

    public IRabbitMqQueueBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqQueueBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings(numberOfInstances);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await queueProvider.CreateQueueAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await queueProvider.CreateQueueAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created
        
        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(int? numberOfInstances = null)
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            SubscribeToSource = _queueName,
            NumberOfInstances = numberOfInstances ?? 1,
        };
    }
}