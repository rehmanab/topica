using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Pulsar.Contracts
{
    public interface IPulsarTopicCreationBuilder
    {
        IPulsarTopicCreationBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IPulsarTopicCreationBuilderWithTopicName
    {
        IPulsarTopicCreationBuilderWithQueues WithTopicName(string topicName);
    }

    public interface IPulsarTopicCreationBuilderWithQueues
    {
        IPulsarTopicCreationBuilderWithConfiguration WithConsumerGroup(string consumerGroup);
    }

    public interface IPulsarTopicCreationBuilderWithConfiguration
    {
        IPulsarTopicCreationBuilderWithOptions WithConfiguration(string tenant, string @namespace, int? numberOfPartitions);
    }

    public interface IPulsarTopicCreationBuilderWithOptions
    {
        IPulsarTopicCreationBuilderWithBuild WithTopicOptions(bool? startNewConsumerEarliest);
    }

    public interface IPulsarTopicCreationBuilderWithBuild
    {
        IPulsarTopicCreationBuilderWithBuild WithProducerOptions(bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds);
        IPulsarTopicCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances);

        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}