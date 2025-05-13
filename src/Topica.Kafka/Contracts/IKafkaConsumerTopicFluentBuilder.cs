using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;

namespace Topica.Kafka.Contracts
{
    public interface IKafkaConsumerTopicFluentBuilder
    {
        IKafkaConsumerTopicBuilderWithTopic WithConsumerName(string consumerName);
    }

    public interface IKafkaConsumerTopicBuilderWithTopic
    {
        IKafkaConsumerTopicBuilderWithQueues WithTopicName(string topicName);
    }
    
    public interface IKafkaConsumerTopicBuilderWithQueues
    {
        IKafkaConsumerTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup);
    }
    
    public interface IKafkaConsumerTopicBuilderWithTopicSettings
    {
        IKafkaConsumerTopicBuilderWithBootstrapServers WithTopicSettings(bool startFromEarliestMessages, int numberOfTopicPartitions);
    }
    
    public interface IKafkaConsumerTopicBuilderWithBootstrapServers
    {
        IKafkaConsumerTopicBuilder WithBootstrapServers(params string[] bootstrapServers);
    }
    
    public interface IKafkaConsumerTopicBuilder
    {
        Task StartConsumingAsync<T>(int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler;
    }
}