using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Kafka.Contracts
{
    public interface IKafkaTopicCreationBuilder
    {
        IKafkaTopicCreationBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IKafkaTopicCreationBuilderWithTopicName
    {
        IKafkaTopicCreationBuilderWithConsumerGroup WithTopicName(string topicName);
    }
    
    public interface IKafkaTopicCreationBuilderWithConsumerGroup
    {
        IKafkaTopicCreationBuilderWithTopicSettings WithConsumerGroup(string consumerGroup);
    }
    
    public interface IKafkaTopicCreationBuilderWithTopicSettings
    {
        IKafkaTopicCreationBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages, int? numberOfTopicPartitions);
    }
    
    public interface IKafkaTopicCreationBuilderWithBootstrapServers
    {
        IKafkaTopicCreationBuilderWithBuild WithBootstrapServers(string[] bootstrapServers);
    }
    
    public interface IKafkaTopicCreationBuilderWithBuild
    {
        IKafkaTopicCreationBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}