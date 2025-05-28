using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicFluentBuilder
    {
        IAwsTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IAwsTopicBuilderWithTopicName
    {
        IAwsTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IAwsTopicBuilderWithQueues
    {
        IAwsTopicBuilder WithSubscribedQueues(string subscribeToQueueName, params string[] queueNames);
    }
    
    public interface IAwsTopicBuilder
    {
        IAwsTopicBuilder WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsTopicBuilder WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsTopicBuilder WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsTopicBuilder WithQueueSettings(int? queueMaximumMessageSize);
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, int? receiveMaximumNumberOfMessages, CancellationToken cancellationToken = default);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}