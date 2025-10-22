using System.Threading;
using System.Threading.Tasks;
using Topica.Aws.Contracts;
using Topica.Aws.Queues;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Aws.Builders;

public class AwsTopicBuilder(ITopicProviderFactory topicProviderFactory) 
    : IAwsTopicBuilder, IAwsTopicBuilderWithTopicName, IAwsTopicBuilderWithQueueToSubscribeTo, IAwsTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _subscribeToQueueName = null!;
    private bool? _isFifoQueue;
    private bool? _isFifoContentBasedDeduplication;
    private int? _numberOfInstances;
    private int? _receiveMaximumNumberOfMessages;

    public IAwsTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAwsTopicBuilderWithQueueToSubscribeTo WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }
    
    public IAwsTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName)
    {
        _subscribeToQueueName = subscribeToQueueName;
        return this;
    }
    
    public IAwsTopicBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication)
    {
        _isFifoQueue = isFifoQueue;
        _isFifoContentBasedDeduplication = isFifoContentBasedDeduplication;
        return this;
    }

    public IAwsTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages)
    {
        _numberOfInstances = numberOfInstances;
        _receiveMaximumNumberOfMessages = receiveMaximumNumberOfMessages;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName);

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Aws);
        var messagingSettings = GetMessagingSettings(_subscribeToQueueName);

        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(string subscribeToQueueName)
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            SubscribeToSource = subscribeToQueueName,
            NumberOfInstances = _numberOfInstances ?? 1,

            AwsIsFifoQueue = _isFifoQueue ?? false,
            AwsWithSubscribedQueues = [],
            AwsBuildWithErrorQueue = false,
            AwsErrorQueueMaxReceiveCount = AwsQueueAttributes.DefaultErrorQueueMaxReceiveCount,
            AwsIsFifoContentBasedDeduplication = _isFifoContentBasedDeduplication ?? false,
            AwsQueueReceiveMaximumNumberOfMessages = (_receiveMaximumNumberOfMessages ?? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages) is < 1 or > 10 ? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages : _receiveMaximumNumberOfMessages ?? AwsQueueAttributes.DefaultQueueReceiveMaximumNumberOfMessages, // Default - 1, (1 - 10)
            AwsMessageVisibilityTimeoutSeconds = AwsQueueAttributes.DefaultMessageVisibilityTimeoutSeconds, // Default - 30 seconds
            AwsQueueMessageDelaySeconds = AwsQueueAttributes.DefaultQueueMessageDelaySeconds, // Default - 0 seconds
            AwsQueueMessageRetentionPeriodSeconds = AwsQueueAttributes.DefaultQueueMessageRetentionPeriodSeconds, // Default - 345600 (4 days)
            AwsQueueReceiveMessageWaitTimeSeconds = AwsQueueAttributes.DefaultQueueReceiveMessageWaitTimeSeconds, // Default - is 10 seconds
            AwsQueueMaximumMessageSizeKb = AwsQueueAttributes.QueueMaximumMessageSizeMaxKb // Default - Between 1 and 262144 bytes (256 KB),
        };
    }
}