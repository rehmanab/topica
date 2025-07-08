using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsQueueCreationBuilder
    {
        IAwsQueueCreationBuilderWithQueueName WithWorkerName(string workerName);
    }

    public interface IAwsQueueCreationBuilderWithQueueName
    {
        IAwsQueueCreationBuilderWithBuild WithQueueName(string queueName);
    }
    
    public interface IAwsQueueCreationBuilderWithBuild
    {
        IAwsQueueCreationBuilderWithBuild WithErrorQueueSettings(bool? buildErrorQueues, int? errorQueueMaxReceiveCount);
        IAwsQueueCreationBuilderWithBuild WithTemporalSettings(int? messageVisibilityTimeoutSeconds, int? queueMessageDelaySeconds, int? queueMessageRetentionPeriodSeconds, int? queueReceiveMessageWaitTimeSeconds);
        IAwsQueueCreationBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsQueueCreationBuilderWithBuild WithQueueSettings(int? queueMaximumMessageSizeKb);
        IAwsQueueCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages);
        
        /// <summary>
        /// Builds the consumer as per the builder settings, ready to send messages.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Returns an <see cref="IConsumer"/> instance, ready to consume the SubscribeToQueueName app setting</returns>
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Builds the producer as per the builder settings, ready to send messages to the Queue.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns><see cref="IProducer"/></returns>
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}