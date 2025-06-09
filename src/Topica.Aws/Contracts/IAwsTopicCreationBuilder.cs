using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicCreationBuilder
    {
        IAwsTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IAwsTopicBuilderWithTopicName
    {
        IAwsTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IAwsTopicBuilderWithQueues
    {
        IAwsTopicBuilderWithQueueToSubscribeTo WithSubscribedQueues(params string[] queueNames);
    }
    
    public interface IAwsTopicBuilderWithQueueToSubscribeTo
    {
        IAwsTopicBuilder WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IAwsTopicBuilder
    {
        IAwsTopicBuilder WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsTopicBuilder WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsTopicBuilder WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsTopicBuilder WithQueueSettings(int? queueMaximumMessageSizeKb);
        IAwsTopicBuilder WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages);
        
        /// <summary>
        /// Builds the consumer with the specified number of instances and maximum number of messages to receive.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns an <see cref="IConsumer"/> instance, ready to consume the SubscribeToQueueName app setting</returns>
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Builds the producer, ready to send messages to the topic.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see cref="IProducer"/></returns>
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}