using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Aws.Contracts
{
    public interface IAwsTopicBuilder
    {
        IAwsTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IAwsTopicBuilderWithTopicName
    {
        IAwsTopicBuilderWithQueueToSubscribeTo WithTopicName(string topicName);
    }
    
    public interface IAwsTopicBuilderWithQueueToSubscribeTo
    {
        IAwsTopicBuilderWithBuild WithQueueToSubscribeTo(string subscribeToQueueName);
    }
    
    public interface IAwsTopicBuilderWithBuild
    {
        IAwsTopicBuilderWithBuild WithFifoSettings(bool? isFifoQueue, bool? isFifoContentBasedDeduplication);
        IAwsTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances, int? receiveMaximumNumberOfMessages);
        
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