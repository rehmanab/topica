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
        IKafkaTopicBuilder WithBootstrapServers(string[] bootstrapServers);
    }
    
    public interface IKafkaTopicBuilder
    {
        IKafkaTopicBuilder WithConsumeSettings(int? numberOfInstances);
        
        Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken);
        Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken);
    }
}