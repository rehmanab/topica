using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqTopicFluentBuilder(ITopicProviderFactory topicProviderFactory, ILogger<RabbitMqTopicFluentBuilder> logger) : IRabbitMqTopicFluentBuilder, IRabbitMqConsumerTopicBuilderWithTopic, IRabbitMqConsumerTopicBuilderWithQueues, IRabbitMqConsumerTopicBuilder
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;
    private string _subscribeToQueueName = null!;

    public IRabbitMqConsumerTopicBuilderWithTopic WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqConsumerTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IRabbitMqConsumerTopicBuilder WithSubscribedQueues(string subscribeToQueueName, params string[] queueNames)
    {
        _subscribeToQueueName = subscribeToQueueName;
        _queueNames = queueNames;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName, numberOfInstances);
        
        logger.LogInformation("***** Connecting {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await topicProvider.CreateTopicAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created

        return await topicProvider.ProvideConsumerAsync(_workerName, messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName);

        logger.LogInformation("***** Connecting {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await topicProvider.CreateTopicAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(string subscribeToQueueName, int? numberOfInstances = null)
    {
        return new MessagingSettings
        {
            Source = _topicName,
            RabbitMqWithSubscribedQueues = _queueNames,
            SubscribeToSource = subscribeToQueueName ?? throw new ArgumentNullException(nameof(subscribeToQueueName), "SubscribeToQueueName cannot be null"),
            NumberOfInstances = numberOfInstances ?? 1,
        };
    }
}