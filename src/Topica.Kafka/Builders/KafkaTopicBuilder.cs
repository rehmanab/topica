using System.Threading;
using System.Threading.Tasks;
using Topica.Kafka.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Builders;

public class KafkaTopicBuilder(
    ITopicProviderFactory topicProviderFactory) 
    : IKafkaTopicBuilder, IKafkaTopicBuilderWithTopicName, IKafkaTopicBuilderWithConsumerGroup, IKafkaTopicBuilderWithTopicSettings, IKafkaTopicBuilderWithBootstrapServers, IKafkaTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private bool? _startFromEarliestMessages;
    private string[] _bootstrapServers = null!;
    private int? _numberOfInstances;

    public IKafkaTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IKafkaTopicBuilderWithConsumerGroup WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IKafkaTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IKafkaTopicBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages)
    {
        _startFromEarliestMessages = startFromEarliestMessages;
        return this;
    }

    public IKafkaTopicBuilderWithBuild WithBootstrapServers(string[] bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
        return this; 
    }

    public IKafkaTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        var messagingSettings = GetMessagingSettings();

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            KafkaConsumerGroup = _consumerGroup,
            KafkaStartFromEarliestMessages = _startFromEarliestMessages ?? false,
            KafkaNumberOfTopicPartitions = 6,
            KafkaBootstrapServers = _bootstrapServers,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}