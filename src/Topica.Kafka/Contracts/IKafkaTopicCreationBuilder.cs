using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Kafka.Contracts
{
    public interface IKafkaTopicCreationBuilder
    {
        IKafkaTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IKafkaTopicBuilderWithTopicName
    {
        IKafkaTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IKafkaTopicBuilderWithQueues
    {
        IKafkaTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup);
    }
    
    public interface IKafkaTopicBuilderWithTopicSettings
    {
        IKafkaTopicBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages, int? numberOfTopicPartitions);
    }
    
    public interface IKafkaTopicBuilderWithBootstrapServers
    {
        IKafkaTopicBuilderWithBuild WithBootstrapServers(params string[] bootstrapServers);
    }
    
    public interface IKafkaTopicBuilderWithBuild
    {
        Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}