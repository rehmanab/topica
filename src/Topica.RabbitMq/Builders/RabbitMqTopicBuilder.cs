using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqTopicBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    ILogger<RabbitMqTopicBuilder> logger) 
    : IRabbitMqTopicBuilder, IRabbitMqTopicBuilderWithTopicName, IRabbitMqTopicBuilderWithQueueToSubscribeTo, IRabbitMqTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;
    private string _subscribeToQueueName = null!;
    private int? _numberOfInstances;

    public IRabbitMqTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqTopicBuilderWithQueueToSubscribeTo WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }
    
    public IRabbitMqTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName)
    {
        _subscribeToQueueName = subscribeToQueueName;
        return this;
    }

    public IRabbitMqTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            RabbitMqWithSubscribedQueues = _queueNames,
            SubscribeToSource = _subscribeToQueueName,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}