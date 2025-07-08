using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarTopicBuilder
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
        IPulsarTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace);
    }

    public interface IPulsarTopicBuilderWithOptions
    {
        IPulsarTopicBuilderWithBuild WithTopicOptions(bool? startNewConsumerEarliest);
    }

    public interface IPulsarTopicBuilderWithBuild
    {
        IPulsarTopicBuilderWithBuild WithProducerOptions(bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds);
        IPulsarTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances);

        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}