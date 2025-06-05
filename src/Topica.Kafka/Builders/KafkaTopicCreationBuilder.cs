using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Topica.Kafka.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Kafka.Builders;

public class KafkaTopicCreationBuilder(ITopicProviderFactory topicProviderFactory, ILogger<KafkaTopicCreationBuilder> logger) : IKafkaTopicCreationBuilder, IKafkaTopicBuilderWithTopicName, IKafkaTopicBuilderWithQueues, IKafkaTopicBuilderWithTopicSettings, IKafkaTopicBuilderWithBootstrapServers, IKafkaTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private bool? _startFromEarliestMessages;
    private int? _numberOfTopicPartitions;
    private string[] _bootstrapServers = null!;

    public IKafkaTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IKafkaTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IKafkaTopicBuilderWithTopicSettings WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IKafkaTopicBuilderWithBootstrapServers WithTopicSettings(bool? startFromEarliestMessages, int? numberOfTopicPartitions)
    {
        _startFromEarliestMessages = startFromEarliestMessages;
        _numberOfTopicPartitions = numberOfTopicPartitions;
        return this;
    }

    public IKafkaTopicBuilderWithBuild WithBootstrapServers(params string[] bootstrapServers)
    {
        _bootstrapServers = bootstrapServers;
        return this; 
    }

    public async Task<IConsumer> BuildConsumerAsync(int? numberOfInstances, CancellationToken cancellationToken = default)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        var messagingSettings = GetMessagingSettings(numberOfInstances);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for consumer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Kafka, _workerName, messagingSettings.Source);
        await topicProvider.CreateTopicAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();

        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Kafka);
        
        logger.LogInformation("***** Please Wait - Connecting to {MessagingPlatform} for producer: {Name} to Source: {MessagingSettings}", MessagingPlatform.Kafka, _workerName, messagingSettings.Source);
        await topicProvider.CreateTopicAsync(messagingSettings);
        await Task.Delay(3000, cancellationToken); // Allow time for the topic to be created
        
        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings(int? numberOfInstances = null)
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            KafkaConsumerGroup = _consumerGroup,
            KafkaStartFromEarliestMessages = _startFromEarliestMessages ?? false,
            KafkaNumberOfTopicPartitions = _numberOfTopicPartitions ?? 6,
            KafkaBootstrapServers = _bootstrapServers,
            NumberOfInstances = numberOfInstances ?? 1
        };
    }
}