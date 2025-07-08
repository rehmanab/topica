using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.RabbitMq.Contracts;
using Topica.Settings;

namespace Topica.RabbitMq.Builders;

public class RabbitMqQueueCreationBuilder(
    IPollyRetryService pollyRetryService,
    IQueueProviderFactory queueProviderFactory, 
    ILogger<RabbitMqQueueCreationBuilder> logger) 
    : IRabbitMqQueueCreationBuilder, IRabbitMqQueueCreationBuilderWithQueueName, IRabbitMqQueueCreationBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private int? _numberOfInstances;

    public IRabbitMqQueueCreationBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IRabbitMqQueueCreationBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IRabbitMqQueueCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.RabbitMq);
        var messagingSettings = GetMessagingSettings();
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.RabbitMq, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(RabbitMqQueueCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => queueProvider.CreateQueueAsync(messagingSettings, ct),
            false,
            cancellationToken
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
            ct => queueProvider.CreateQueueAsync(messagingSettings, ct),
            false,
            cancellationToken
        );
        
        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            SubscribeToSource = _queueName,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}