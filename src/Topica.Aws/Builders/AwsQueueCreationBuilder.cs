using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
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
public class AwsQueueCreationBuilder(IQueueProviderFactory queueProviderFactory, ILogger<AwsQueueCreationBuilder> logger) : IAwsQueueCreationBuilder, IAwsQueueBuilderWithQueueName, IAwsQueueBuilderWithBuild
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
    private int? _queueMaximumMessageSize;
    
    public IAwsQueueBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount)
    {
        _buildErrorQueues = buildErrorQueues;
        _errorQueueMaxReceiveCount = errorQueueMaxReceiveCount;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds)
    {
        _messageVisibilityTimeoutSeconds = messageVisibilityTimeoutSeconds;
        _queueMessageDelaySeconds = queueMessageDelaySeconds;
        _queueMessageRetentionPeriodSeconds = queueMessageRetentionPeriodSeconds;
        _queueReceiveMessageWaitTimeSeconds = queueReceiveMessageWaitTimeSeconds;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication)
    {
        _isFifoQueue = isFifoQueue;
        _isFifoContentBasedDeduplication = isFifoContentBasedDeduplication;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithQueueSettings(int? queueMaximumMessageSize)
    {
        _queueMaximumMessageSize = queueMaximumMessageSize;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, int? receiveMaximumNumberOfMessages, CancellationToken cancellationToken = default)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings(numberOfInstances, receiveMaximumNumberOfMessages);

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Aws, _workerName, messagingSettings.Source);
        await queueProvider.CreateQueueAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings();

        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Aws, _workerName, messagingSettings.Source);
        await queueProvider.CreateQueueAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created

        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }
    
    private MessagingSettings GetMessagingSettings(int? numberOfInstances = null, int? receiveMaximumNumberOfMessages = null)
    {
        // TODO - move all variables to the constants file
        var isFifoQueue = _isFifoQueue ?? false;
        var queueMaximumMessageSizeMax = _queueMaximumMessageSize ?? AwsQueueAttributes.QueueMaximumMessageSizeMax;
        var awsQueueReceiveMessageWaitTimeSeconds = _queueReceiveMessageWaitTimeSeconds ?? 0;
        var awsQueueMessageRetentionPeriodSeconds = _queueMessageRetentionPeriodSeconds ?? 345600;
        var awsQueueMessageDelaySeconds = _queueMessageDelaySeconds ?? 0;
        var awsMessageVisibilityTimeoutSeconds = _messageVisibilityTimeoutSeconds ?? AwsQueueAttributes.MessageVisibilityTimeoutSecondsDefault;
        var awsQueueReceiveMaximumNumberOfMessages = receiveMaximumNumberOfMessages ?? 10;
        var awsNumberOfInstances = numberOfInstances ?? 1;

        var queueName = !string.IsNullOrWhiteSpace(_queueName) && isFifoQueue && !_queueName.EndsWith(Constants.FifoSuffix) ? $"{_queueName}{Constants.FifoSuffix}" : _queueName;
        
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = queueName,
            SubscribeToSource = queueName,
            NumberOfInstances = awsNumberOfInstances,

            AwsIsFifoQueue = isFifoQueue,
            AwsBuildWithErrorQueue = _buildErrorQueues ?? false,
            AwsErrorQueueMaxReceiveCount = _errorQueueMaxReceiveCount ?? 5,
            AwsIsFifoContentBasedDeduplication = _isFifoContentBasedDeduplication ?? false,
            AwsQueueReceiveMaximumNumberOfMessages = awsQueueReceiveMaximumNumberOfMessages is < 1 or > 10 ? 1 : awsQueueReceiveMaximumNumberOfMessages, // Default - 1, (1 - 10)
            AwsMessageVisibilityTimeoutSeconds = awsMessageVisibilityTimeoutSeconds is < AwsQueueAttributes.MessageVisibilityTimeoutSecondsMin or > AwsQueueAttributes.MessageVisibilityTimeoutSecondsMax ? 30 : awsMessageVisibilityTimeoutSeconds, // Default - 30 seconds
            AwsQueueMessageDelaySeconds = awsQueueMessageDelaySeconds is < AwsQueueAttributes.QueueMessageDelaySecondsMin or > AwsQueueAttributes.QueueMessageDelaySecondsMax ? 0 : awsQueueMessageDelaySeconds, // Default - 0 seconds
            AwsQueueMessageRetentionPeriodSeconds = awsQueueMessageRetentionPeriodSeconds is < AwsQueueAttributes.QueueMessageRetentionPeriodMin or > AwsQueueAttributes.QueueMessageRetentionPeriodMax ? 345600 : awsQueueMessageRetentionPeriodSeconds, // Default - 345600 (4 days)
            AwsQueueReceiveMessageWaitTimeSeconds = awsQueueReceiveMessageWaitTimeSeconds is < AwsQueueAttributes.QueueReceiveMessageWaitTimeSecondsMin or > AwsQueueAttributes.QueueReceiveMessageWaitTimeSecondsMax ? 0 : awsQueueReceiveMessageWaitTimeSeconds, // Default - is 0 seconds
            AwsQueueMaximumMessageSize = queueMaximumMessageSizeMax is < AwsQueueAttributes.QueueMaximumMessageSizeMin or > AwsQueueAttributes.QueueMaximumMessageSizeMax ? 262144 : queueMaximumMessageSizeMax // Default - Between 1 and 262144 bytes (256 KB),
        };
    }
}