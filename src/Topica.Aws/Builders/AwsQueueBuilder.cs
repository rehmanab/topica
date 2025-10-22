using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Builders;

public class AwsQueueBuilder(
    IQueueProviderFactory queueProviderFactory,
    ILogger<AwsQueueCreationBuilder> logger) 
    : IAwsQueueBuilder, IAwsQueueBuilderWithQueueName, IAwsQueueBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private bool? _isFifoQueue;
    private bool? _isFifoContentBasedDeduplication;
    private int? _numberOfInstances;
    private int? _receiveMaximumNumberOfMessages;

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

    public IAwsQueueBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication)
    {
        _isFifoQueue = isFifoQueue;
        _isFifoContentBasedDeduplication = isFifoContentBasedDeduplication;
        return this;
    }

    public IAwsQueueBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages)
    {
        _numberOfInstances = numberOfInstances;
        _receiveMaximumNumberOfMessages = receiveMaximumNumberOfMessages;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);

        return await queueProvider.ProvideConsumerAsync(GetMessagingSettings());
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.Aws);

        return await queueProvider.ProvideProducerAsync(_workerName, GetMessagingSettings());
    }
        
    private MessagingSettings GetMessagingSettings()
    {
        var isFifoQueue = _isFifoQueue ?? false;
        var awsNumberOfInstances = _numberOfInstances ?? 1;
        var awsQueueReceiveMaximumNumberOfMessages = _receiveMaximumNumberOfMessages ?? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages;

        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            SubscribeToSource = _queueName,
            NumberOfInstances = awsNumberOfInstances,

            AwsIsFifoQueue = isFifoQueue,
            AwsBuildWithErrorQueue = false,
            AwsErrorQueueMaxReceiveCount = AwsQueueAttributes.DefaultErrorQueueMaxReceiveCount,
            AwsIsFifoContentBasedDeduplication = _isFifoContentBasedDeduplication ?? false,
            AwsQueueReceiveMaximumNumberOfMessages = awsQueueReceiveMaximumNumberOfMessages is < 1 or > 10 ? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages : awsQueueReceiveMaximumNumberOfMessages, // Default - 1, (1 - 10)
            AwsMessageVisibilityTimeoutSeconds = AwsQueueAttributes.DefaultMessageVisibilityTimeoutSeconds, // Default - 30 seconds
            AwsQueueMessageDelaySeconds = AwsQueueAttributes.DefaultQueueMessageDelaySeconds, // Default - 0 seconds
            AwsQueueMessageRetentionPeriodSeconds = AwsQueueAttributes.DefaultQueueMessageRetentionPeriodSeconds, // Default - 345600 (4 days)
            AwsQueueReceiveMessageWaitTimeSeconds = AwsQueueAttributes.DefaultQueueReceiveMessageWaitTimeSeconds, // Default - is 10 seconds
            AwsQueueMaximumMessageSizeKb = AwsQueueAttributes.QueueMaximumMessageSizeMaxKb // Default - Between 1 and 262144 bytes (256 KB),
        };
    }
}