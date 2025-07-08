using System.Threading;
using System.Threading.Tasks;
using Topica.Contracts;
using Topica.Pulsar.Contracts;
using Topica.Settings;

namespace Topica.Pulsar.Builders;

public class PulsarTopicBuilder(
    ITopicProviderFactory topicProviderFactory) 
    : IPulsarTopicBuilder, IPulsarTopicBuilderWithTopicName, IPulsarTopicBuilderWithQueues, IPulsarTopicBuilderWithConfiguration, IPulsarTopicBuilderWithOptions, IPulsarTopicBuilderWithBuild
{
    private string _workerName = null!;
    private string _topicName = null!;
    private string _consumerGroup = null!;
    private string _tenant = null!;
    private string _namespace = null!;
    private bool? _startNewConsumerEarliest;
    private bool? _blockIfQueueFull;
    private int? _maxPendingMessages;
    private int? _maxPendingMessagesAcrossPartitions;
    private bool? _enableBatching;
    private bool? _enableChunking;
    private int? _batchingMaxMessages;
    private long? _batchingMaxPublishDelayMilliseconds;
    private int? _numberOfInstances;

    public IPulsarTopicBuilderWithTopicName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IPulsarTopicBuilderWithQueues WithTopicName(string topicName)
    {
        _topicName = topicName;
        return this;
    }

    public IPulsarTopicBuilderWithConfiguration WithConsumerGroup(string consumerGroup)
    {
        _consumerGroup = consumerGroup;
        return this;
    }

    public IPulsarTopicBuilderWithOptions WithConfiguration(string tenant, string @namespace)
    {
        _tenant = tenant;
        _namespace = @namespace;
        return this;
    }
        
    public IPulsarTopicBuilderWithBuild WithTopicOptions(bool? startNewConsumerEarliest)
    {
        _startNewConsumerEarliest = startNewConsumerEarliest;
        return this;
    }

    public IPulsarTopicBuilderWithBuild WithProducerOptions(bool? blockIfQueueFull, int? maxPendingMessages, int? maxPendingMessagesAcrossPartitions, bool? enableBatching, bool? enableChunking, int? batchingMaxMessages, long? batchingMaxPublishDelayMilliseconds)
    {
        _blockIfQueueFull = blockIfQueueFull;
        _maxPendingMessages = maxPendingMessages;
        _maxPendingMessagesAcrossPartitions = maxPendingMessagesAcrossPartitions;
        _enableBatching = enableBatching;
        _enableChunking = enableChunking;
        _batchingMaxMessages = batchingMaxMessages;
        _batchingMaxPublishDelayMilliseconds = batchingMaxPublishDelayMilliseconds;
        return this;
    }

    public IPulsarTopicBuilderWithBuild WithConsumeSettings(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Pulsar);
        var messagingSettings = GetMessagingSettings();

        return await topicProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var topicProvider = topicProviderFactory.Create(MessagingPlatform.Pulsar);
        var messagingSettings = GetMessagingSettings();
        messagingSettings.PulsarBlockIfQueueFull = _blockIfQueueFull ?? true;
        messagingSettings.PulsarMaxPendingMessages = _maxPendingMessages ?? int.MaxValue;
        messagingSettings.PulsarMaxPendingMessagesAcrossPartitions = _maxPendingMessagesAcrossPartitions ?? int.MaxValue;
        messagingSettings.PulsarEnableBatching = _enableBatching ?? false;
        messagingSettings.PulsarEnableChunking = _enableChunking ?? false;
        messagingSettings.PulsarBatchingMaxMessages = _batchingMaxMessages ?? 10;
        messagingSettings.PulsarBatchingMaxPublishDelayMilliseconds = _batchingMaxPublishDelayMilliseconds ?? 500;

        return await topicProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _topicName,
            PulsarTenant = _tenant,
            PulsarNamespace = _namespace,
            PulsarConsumerGroup = _consumerGroup,
            PulsarStartNewConsumerEarliest = _startNewConsumerEarliest ?? false,
            PulsarTopicNumberOfPartitions = 6,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}