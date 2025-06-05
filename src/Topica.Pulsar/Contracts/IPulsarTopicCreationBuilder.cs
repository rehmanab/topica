using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarTopicCreationBuilder
    {
        IPulsarConsumerTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IPulsarConsumerTopicBuilderWithTopicName
    {
        IPulsarConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IPulsarConsumerTopicBuilderWithQueues
    {
        IPulsarConsumerTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup);
    }
    
    public interface IPulsarConsumerTopicBuilderWithConfiguration
    {
        IPulsarConsumerTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace, int? numberOfPartitions);
    }
    
    public interface IPulsarConsumerTopicBuilderWithOptions
    {
        IPulsarConsumerTopicBuilderWithBuild WithTopicOptions(bool? startNewConsumerEarliest);
    }
    
    public interface IPulsarConsumerTopicBuilderWithBuild
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        IPulsarConsumerTopicBuilderWithBuild WithProducerOptions( bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}