using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueCreationBuilder
    {
        IAwsQueueBuilderWithQueueName WithWorkerName(string workerName);
    }

    public interface IAwsQueueBuilderWithQueueName
    {
        IAwsQueueBuilder WithQueueName(string queueName);
    }
    
    public interface IAwsQueueBuilder
    {
        IAwsQueueBuilder WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsQueueBuilder WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsQueueBuilder WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsQueueBuilder WithQueueSettings(int? queueMaximumMessageSizeKb);
        IAwsQueueBuilder WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages);
        
        /// <summary>
        /// Builds the consumer with the specified number of instances and maximum number of messages to receive.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns an <see cref="IConsumer"/> instance, ready to consume the SubscribeToQueueName app setting</returns>
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Builds the producer, ready to send messages to the Queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see cref="IProducer"/></returns>
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}