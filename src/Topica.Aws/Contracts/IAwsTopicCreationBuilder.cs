using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicCreationBuilder
    {
        IAwsTopicCreationBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IAwsTopicCreationBuilderWithTopicName
    {
        IAwsTopicCreationBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IAwsTopicCreationBuilderWithQueues
    {
        IAwsTopicCreationBuilderWithQueueToSubscribeTo WithSubscribedQueues(string[] queueNames);
    }
    
    public interface IAwsTopicCreationBuilderWithQueueToSubscribeTo
    {
        IAwsTopicCreationBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IAwsTopicCreationBuilderWithBuild
    {
        IAwsTopicCreationBuilderWithBuild WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsTopicCreationBuilderWithBuild WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsTopicCreationBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsTopicCreationBuilderWithBuild WithQueueSettings(int? queueMaximumMessageSizeKb);
        IAwsTopicCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages);
        
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