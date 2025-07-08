using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqTopicCreationBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    ILogger<RabbitMqTopicCreationBuilder> logger) 
    : IRabbitMqTopicCreationBuilder, IRabbitMqTopicCreationBuilderWithTopicName, IRabbitMqTopicCreationBuilderWithQueues, IRabbitMqTopicCreationBuilderWithQueueToSubscribeTo, IRabbitMqTopicCreationBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string[] _queueNames = null!;
    private string _subscribeToQueueName = null!;
    private int? _numberOfInstances;

    public IRabbitMqTopicCreationBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqTopicCreationBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IRabbitMqTopicCreationBuilderWithQueueToSubscribeTo WithSubscribedQueues(string[] queueNames)
    {
        _queueNames = queueNames;
        return this;
    }
    
    public IRabbitMqTopicCreationBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName)
    {
        _subscribeToQueueName = subscribeToQueueName;
        return this;
    }

    public IRabbitMqTopicCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => topicProvider.CreateTopicAsync(messagingSettings, ct),
            false,
            cancellationToken
        );

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => topicProvider.CreateTopicAsync(messagingSettings, ct),
            false,
            cancellationToken
        );
        
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