using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Builders;

public class PulsarTopicCreationBuilder(
    IPollyRetryService pollyRetryService,
    ITopicProviderFactory topicProviderFactory, 
    ILogger<PulsarTopicCreationBuilder> logger) 
    : IPulsarTopicCreationBuilder, IPulsarTopicBuilderWithTopicName, IPulsarTopicBuilderWithQueues, IPulsarTopicBuilderWithConfiguration, IPulsarTopicBuilderWithOptions, IPulsarTopicBuilder
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private string _tenant = null!;
    private string _namespace = null!;
    private int? _numberOfPartitions;
    private bool? _startNewConsumerEarliest;
    private bool? _blockIfQueueFull;
    private int? _maxPendingMessages;
    private int? _maxPendingMessagesAcrossPartitions;
    private bool? _enableBatching;
    private bool? _enableChunking;
    private int? _batchingMaxMessages;
    private long? _batchingMaxPublishDelayMilliseconds;
    private int? _numberOfInstances;

    public IPulsarTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IPulsarTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IPulsarTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IPulsarTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace, int? numberOfPartitions)
    {
        _tenant = tenant;
        _namespace = @namespace;
        _numberOfPartitions = numberOfPartitions;
        return this;
    }
        
    public IPulsarTopicBuilder WithTopicOptions(bool? startNewConsumerEarliest)
    {
        _startNewConsumerEarliest = startNewConsumerEarliest;
        return this;
    }

    public IPulsarTopicBuilder WithProducerOptions(bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds)
    {
        _blockIfQueueFull = blockIfQueueFull;
        _maxPendingMessages = maxPendingMessages;
        _maxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
        _enableBatching = enableBatching;
        _enableChunking = enableChunking;
        _batchingMaxMessages = batchingMaxMessages;
        _batchingMaxPublishDelayMilliseconds = batchingMaxPublishDelayMilliseconds;
        return this;
    }

    public IPulsarTopicBuilder WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Pulsar);
        var messagingSettings = GetMessagingSettings();
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Pulsar, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(PulsarTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => topicProvider.CreateTopicAsync(messagingSettings, ct),
            false,
            cancellationToken
        );

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();
        messagingSettings.PulsarBlockIfQueueFull = _blockIfQueueFull ?? true;
        messagingSettings.PulsarMaxPendingMessages = _maxPendingMessages ?? int.MaxValue;
        messagingSettings.PulsarMaxPendingMessagesAcrossPartitions = _maxPendingMessagesAcrossPartitions ?? int.MaxValue;
        messagingSettings.PulsarEnableBatching = _enableBatching ?? false;
        messagingSettings.PulsarEnableChunking = _enableChunking ?? false;
        messagingSettings.PulsarBatchingMaxMessages = _batchingMaxMessages ?? 10;
        messagingSettings.PulsarBatchingMaxPublishDelayMilliseconds = _batchingMaxPublishDelayMilliseconds ?? 500;

        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Pulsar);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Pulsar, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<Exception>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(PulsarTopicCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
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
            PulsarTenant = _tenant,
            PulsarNamespace = _namespace,
            PulsarConsumerGroup = _consumerGroup,
            PulsarStartNewConsumerEarliest = _startNewConsumerEarliest ?? false,
            PulsarTopicNumberOfPartitions = _numberOfPartitions ?? 6,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}