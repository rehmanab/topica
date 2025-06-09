using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Infrastructure.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqQueueCreationBuilder(
    IPollyRetryService pollyRetryService,
    IQueueProviderFactory queueProviderFactory, 
    ILogger<RabbitMqTopicCreationBuilder> logger) 
    : IRabbitMqQueueCreationBuilder, IRabbitMqQueueBuilderWithQueueName, IRabbitMqQueueBuilderWithBuild
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
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqQueueCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => queueProvider.CreateQueueAsync(messagingSettings),
            false
        );

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {        
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqQueueCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            () => queueProvider.CreateQueueAsync(messagingSettings),
            false
        );
        
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