using System.Threading;
using System.Threading.Tasks;
using Topica.Kafka.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Builders;

public class KafkaConsumerTopicBuilder(IConsumer consumer) : IKafkaConsumerTopicFluentBuilder, IKafkaConsumerTopicBuilderWithTopic, IKafkaConsumerTopicBuilderWithQueues, IKafkaConsumerTopicBuilderWithTopicSettings, IKafkaConsumerTopicBuilderWithBootstrapServers, IKafkaConsumerTopicBuilder
{
    private string _consumerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private bool _startFromEarliestMessages;
    private int _numberOfTopicPartitions;
    private string[] _bootstrapServers = null!;

    public IKafkaConsumerTopicBuilderWithTopic WithConsumerName(string consumerName)
    {
        _consumerName = consumerName;
        return this;
    }

    public IKafkaConsumerTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IKafkaConsumerTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IKafkaConsumerTopicBuilderWithBootstrapServers WithTopicSettings(bool startFromEarliestMessages, int numberOfTopicPartitions)
    {
        _startFromEarliestMessages = startFromEarliestMessages;
        _numberOfTopicPartitions = numberOfTopicPartitions;
        return this;
    }

    public IKafkaConsumerTopicBuilder WithBootstrapServers(params string[] bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
        return this; 
    }

    public async Task StartConsumingAsync<T>(int numberOfInstances, CancellationToken cancellationToken = default) where T : class, IHandler
    {
        var instances = numberOfInstances switch
        {
            < 1 => 1,
            > 10 => 10,
            _ => numberOfInstances
        };

        var consumerSettings = new ConsumerSettings
        {
            Source = _topicName,
            KafkaConsumerGroup = _consumerGroup,
            KafkaStartFromEarliestMessages = _startFromEarliestMessages,
            KafkaNumberOfTopicPartitions = _numberOfTopicPartitions,
            KafkaBootstrapServers = _bootstrapServers,
            NumberOfInstances = instances
        };

        await consumer.ConsumeAsync<T>(_consumerName, consumerSettings, cancellationToken);
    }
}