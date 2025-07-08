using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Builders;

/// <summary>
/// Builds an AWS queue with various settings.
/// BuildConsumerAsync method creates the queue and returns a consumer.
/// BuildProducerAsync method creates the queue and returns a producer.
/// </summary>
/// <param name="queueProviderFactory"></param>
/// <param name="logger"></param>
public class AwsQueueCreationBuilder(IQueueProviderFactory queueProviderFactory, 
    IPollyRetryService pollyRetryService,
    ILogger<AwsQueueCreationBuilder> logger) 
    : IAwsQueueCreationBuilder, IAwsQueueCreationBuilderWithQueueName, IAwsQueueCreationBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private bool? _buildErrorQueues;
    private int? _errorQueueMaxReceiveCount;
    private bool? _isFifoQueue;
    private bool? _isFifoContentBasedDeduplication;
    private int? _messageVisibilityTimeoutSeconds;
    private int? _queueMessageDelaySeconds;
    private int? _queueMessageRetentionPeriodSeconds;
    private int? _queueReceiveMessageWaitTimeSeconds;
    private int? _queueMaximumMessageSizeKb;
    private int? _numberOfInstances;
    private int? _receiveMaximumNumberOfMessages;

    public IAwsQueueCreationBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount)
    {
        _buildErrorQueues = buildErrorQueues;
        _errorQueueMaxReceiveCount = errorQueueMaxReceiveCount;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds)
    {
        _messageVisibilityTimeoutSeconds = messageVisibilityTimeoutSeconds;
        _queueMessageDelaySeconds = queueMessageDelaySeconds;
        _queueMessageRetentionPeriodSeconds = queueMessageRetentionPeriodSeconds;
        _queueReceiveMessageWaitTimeSeconds = queueReceiveMessageWaitTimeSeconds;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication)
    {
        _isFifoQueue = isFifoQueue;
        _isFifoContentBasedDeduplication = isFifoContentBasedDeduplication;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithQueueSettings(int? queueMaximumMessageSizeKb)
    {
        _queueMaximumMessageSizeKb = queueMaximumMessageSizeKb;
        return this;
    }

    public IAwsQueueCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages)
    {
        _numberOfInstances = numberOfInstances;
        _receiveMaximumNumberOfMessages = receiveMaximumNumberOfMessages;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Aws, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<QueueDeletedRecentlyException>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(AwsQueueCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => queueProvider.CreateQueueAsync(messagingSettings, ct),
            false,
            cancellationToken
        );
        
        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Aws, _workerName, messagingSettings.Source);
        await pollyRetryService.WaitAndRetryAsync<QueueDeletedRecentlyException>
        (
            30,
            _ => TimeSpan.FromSeconds(10),
            (delegateResult, ts, index, context) => logger.LogWarning("**** RETRY: {Name}:  Retry attempt: {RetryAttempt} - Retry in {RetryDelayTotalSeconds} - Error ({ExceptionType}) Message: {Result}", nameof(AwsQueueCreationBuilder), index, ts, delegateResult.GetType(), delegateResult.Message ?? "Error creating queue."),
            ct => queueProvider.CreateQueueAsync(messagingSettings, ct),
            false,
            cancellationToken
        );

        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }
    
    private MessagingSettings GetMessagingSettings()
    {
        var isFifoQueue = _isFifoQueue ?? false;
        var queueMaximumMessageSizeMaxKb = _queueMaximumMessageSizeKb ?? AwsQueueAttributes.QueueMaximumMessageSizeMaxKb;
        var awsQueueReceiveMessageWaitTimeSeconds = _queueReceiveMessageWaitTimeSeconds ?? AwsQueueAttributes.DefaultQueueReceiveMessageWaitTimeSeconds;
        var awsQueueMessageRetentionPeriodSeconds = _queueMessageRetentionPeriodSeconds ?? AwsQueueAttributes.DefaultQueueMessageRetentionPeriodSeconds;
        var awsQueueMessageDelaySeconds = _queueMessageDelaySeconds ?? AwsQueueAttributes.DefaultQueueMessageDelaySeconds;
        var awsMessageVisibilityTimeoutSeconds = _messageVisibilityTimeoutSeconds ?? AwsQueueAttributes.DefaultMessageVisibilityTimeoutSeconds;
        var awsNumberOfInstances = _numberOfInstances ?? 1;
        var awsQueueReceiveMaximumNumberOfMessages = _receiveMaximumNumberOfMessages ?? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages;

        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            SubscribeToSource = _queueName,
            NumberOfInstances = awsNumberOfInstances,

            AwsIsFifoQueue = isFifoQueue,
            AwsBuildWithErrorQueue = _buildErrorQueues ?? false,
            AwsErrorQueueMaxReceiveCount = _errorQueueMaxReceiveCount ?? AwsQueueAttributes.DefaultErrorQueueMaxReceiveCount,
            AwsIsFifoContentBasedDeduplication = _isFifoContentBasedDeduplication ?? false,
            AwsQueueReceiveMaximumNumberOfMessages = awsQueueReceiveMaximumNumberOfMessages is < 1 or > 10 ? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages : awsQueueReceiveMaximumNumberOfMessages, // Default - 1, (1 - 10)
            AwsMessageVisibilityTimeoutSeconds = awsMessageVisibilityTimeoutSeconds is < AwsQueueAttributes.MessageVisibilityTimeoutSecondsMin or > AwsQueueAttributes.MessageVisibilityTimeoutSecondsMax ? AwsQueueAttributes.DefaultMessageVisibilityTimeoutSeconds : awsMessageVisibilityTimeoutSeconds, // Default - 30 seconds
            AwsQueueMessageDelaySeconds = awsQueueMessageDelaySeconds is < AwsQueueAttributes.QueueMessageDelaySecondsMin or > AwsQueueAttributes.QueueMessageDelaySecondsMax ? AwsQueueAttributes.DefaultQueueMessageDelaySeconds : awsQueueMessageDelaySeconds, // Default - 0 seconds
            AwsQueueMessageRetentionPeriodSeconds = awsQueueMessageRetentionPeriodSeconds is < AwsQueueAttributes.QueueMessageRetentionPeriodSecondsMin or > AwsQueueAttributes.QueueMessageRetentionPeriodSecondsMax ? AwsQueueAttributes.DefaultQueueMessageRetentionPeriodSeconds : awsQueueMessageRetentionPeriodSeconds, // Default - 345600 (4 days)
            AwsQueueReceiveMessageWaitTimeSeconds = awsQueueReceiveMessageWaitTimeSeconds is < AwsQueueAttributes.QueueReceiveMessageWaitTimeSecondsMin or > AwsQueueAttributes.QueueReceiveMessageWaitTimeSecondsMax ? AwsQueueAttributes.DefaultQueueReceiveMessageWaitTimeSeconds : awsQueueReceiveMessageWaitTimeSeconds, // Default - is 0 seconds
            AwsQueueMaximumMessageSizeKb = queueMaximumMessageSizeMaxKb is < AwsQueueAttributes.QueueMaximumMessageSizeMinKb or > AwsQueueAttributes.QueueMaximumMessageSizeMaxKb ? AwsQueueAttributes.DefaultQueueMaximumMessageSizeMaxKb : queueMaximumMessageSizeMaxKb // Default - Between 1 and 262144 bytes (256 KB),
        };
    }
}