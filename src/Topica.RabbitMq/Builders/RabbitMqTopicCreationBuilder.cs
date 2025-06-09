using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Infrastructure.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqTopicCreationBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    ILogger<RabbitMqTopicCreationBuilder> logger) 
    : IRabbitMqTopicCreationBuilder, IRabbitMqTopicBuilderWithTopicName, IRabbitMqTopicBuilderWithQueues, IRabbitMqTopicBuilderWithQueueToSubscribeTo, IRabbitMqTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;
    private string _subscribeToQueueName = null!;

    public IRabbitMqTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IRabbitMqTopicBuilderWithQueueToSubscribeTo WithSubscribedQueues(params string[] queueNames)
    {
        _queueNames = queueNames;
        return this;
    }
    
    public IRabbitMqTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName)
    {
        _subscribeToQueueName = subscribeToQueueName;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName, numberOfInstances);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => topicProvider.CreateTopicAsync(messagingSettings),
            false
        );

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName);

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => topicProvider.CreateTopicAsync(messagingSettings),
            false
        );
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(string subscribeToQueueName, int? numberOfInstances = null)
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            RabbitMqWithSubscribedQueues = _queueNames,
            SubscribeToSource = subscribeToQueueName,
            NumberOfInstances = numberOfInstances ?? 1,
        };
    }
}