using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarTopicCreationBuilder
    {
        IPulsarTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IPulsarTopicBuilderWithTopicName
    {
        IPulsarTopicBuilderWithQueues WithTopicName(string topicName);
    }

    public interface IPulsarTopicBuilderWithQueues
    {
        IPulsarTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup);
    }

    public interface IPulsarTopicBuilderWithConfiguration
    {
        IPulsarTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace, int? numberOfPartitions);
    }

    public interface IPulsarTopicBuilderWithOptions
    {
        IPulsarTopicBuilder WithTopicOptions(bool? startNewConsumerEarliest);
    }

    public interface IPulsarTopicBuilder
    {
        IPulsarTopicBuilder WithProducerOptions(bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds);
        IPulsarTopicBuilder WithConsumeSettings(int? numberOfInstances);

        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}