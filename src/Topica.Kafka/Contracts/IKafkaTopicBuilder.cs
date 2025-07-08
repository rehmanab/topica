using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Kafka.Contracts
{
    public interface IKafkaTopicBuilder
    {
        IKafkaTopicBuilderWithTopicName WithWorkerName(string workerName);
    }

    public interface IKafkaTopicBuilderWithTopicName
    {
        IKafkaTopicBuilderWithConsumerGroup WithTopicName(string topicName);
    }
    
    public interface IKafkaTopicBuilderWithConsumerGroup
    {
        IKafkaTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup);
    }
    
    public interface IKafkaTopicBuilderWithTopicSettings
    {
        IKafkaTopicBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages);
    }
    
    public interface IKafkaTopicBuilderWithBootstrapServers
    {
        IKafkaTopicBuilderWithBuild WithBootstrapServers(string[] bootstrapServers);
    }
    
    public interface IKafkaTopicBuilderWithBuild
    {
        IKafkaTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}