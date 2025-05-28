using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarConsumerTopicFluentBuilder
    {
        IPulsarConsumerTopicBuilderWithTopic WithWorkerName(string workerName);
    }

    public interface IPulsarConsumerTopicBuilderWithTopic
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
        IPulsarConsumerTopicBuilder WithTopicOptions(bool? startNewConsumerEarliest);
    }
    
    public interface IPulsarConsumerTopicBuilder
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        IPulsarConsumerTopicBuilder WithProducerOptions( bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}