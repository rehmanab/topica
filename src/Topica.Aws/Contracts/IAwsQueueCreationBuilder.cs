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
        IAwsQueueBuilderWithBuild WithQueueName(string queueName);
    }
    
    public interface IAwsQueueBuilderWithBuild
    {
        IAwsQueueBuilderWithBuild WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsQueueBuilderWithBuild WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsQueueBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsQueueBuilderWithBuild WithQueueSettings(int? queueMaximumMessageSize);
        
        /// <summary>
        /// Builds the consumer with the specified number of instances and maximum number of messages to receive.
        /// </summary>
        /// <param name="numberOfInstances">Number of parallel instance</param>
        /// <param name="receiveMaximumNumberOfMessages">How many AWS messages to get per receive</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns an <see cref="IConsumer"/> instance, ready to consume the SubscribeToQueueName app setting</returns>
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, int? receiveMaximumNumberOfMessages, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Builds the producer, ready to send messages to the Queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see cref="IProducer"/></returns>
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}