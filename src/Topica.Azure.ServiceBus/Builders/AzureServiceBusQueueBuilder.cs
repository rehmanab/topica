using Topica.Azure.ServiceBus.Contracts;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Builders;

public class AzureServiceBusQueueBuilder(IQueueProviderFactory queueProviderFactory)
    : IAzureServiceBusQueueBuilder, IAzureServiceBusQueueBuilderWithQueueName, IAzureServiceBusQueueBuilderWithBuild
{
    private string _workerName = null!;
    private string _queueName = null!;
    private string? _duplicateDetectionHistoryTimeWindow;
    private bool? _enableBatchedOperations = true;
    private bool? _enablePartitioning = false;
    private int? _maxSizeInMegabytes = 1024;
    private int? _maxMessageSizeInKilobytes = 256;
    private bool? _enabledStatus = true;
    private bool? _supportOrdering = false;
    private string? _metadata;
    private int? _numberOfInstances;

    public IAzureServiceBusQueueBuilderWithQueueName WithWorkerName(string workerName)
    {
        _workerName = workerName;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithQueueName(string queueName)
    {
        _queueName = queueName;
        return this;
    }
    
    public IAzureServiceBusQueueBuilderWithBuild WithTimings(
        string? duplicateDetectionHistoryTimeWindow)
    {
        _duplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithOptions(
        bool? enableBatchedOperations, 
        bool? enablePartitioning,
        int? maxSizeInMegabytes, 
        int? maxMessageSizeInKilobytes, 
        bool? enabledStatus,
        bool? supportOrdering)
    {
        _enableBatchedOperations = enableBatchedOperations;
        _enablePartitioning = enablePartitioning;
        _maxSizeInMegabytes = maxSizeInMegabytes;
        _maxMessageSizeInKilobytes = maxMessageSizeInKilobytes;
        _enabledStatus = enabledStatus;
        _supportOrdering = supportOrdering;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithMetadata(string? metadata)
    {
        _metadata = metadata;
        return this;
    }

    public IAzureServiceBusQueueBuilderWithBuild WithNumberOfInstances(int? numberOfInstances)
    {
        _numberOfInstances = numberOfInstances;
        return this;
    }

    public async Task<IConsumer> BuildConsumerAsync(CancellationToken cancellationToken)
    {
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.AzureServiceBus);
        var messagingSettings = GetMessagingSettings();

        return await queueProvider.ProvideConsumerAsync(messagingSettings);
    }

    public async Task<IProducer> BuildProducerAsync(CancellationToken cancellationToken)
    {
        var messagingSettings = GetMessagingSettings();
        var queueProvider = queueProviderFactory.Create(MessagingPlatform.AzureServiceBus);

        return await queueProvider.ProvideProducerAsync(_workerName, messagingSettings);
    }

    private MessagingSettings GetMessagingSettings()
    {
        return new MessagingSettings
        {
            WorkerName = _workerName,
            Source = _queueName,
            AzureServiceBusDuplicateDetectionHistoryTimeWindow = _duplicateDetectionHistoryTimeWindow,
            AzureServiceBusEnableBatchedOperations = _enableBatchedOperations,
            AzureServiceBusEnablePartitioning = _enablePartitioning,
            AzureServiceBusMaxSizeInMegabytes = _maxSizeInMegabytes,
            AzureServiceBusMaxMessageSizeInKilobytes = _maxMessageSizeInKilobytes,
            AzureServiceBusEnabledStatus = _enabledStatus,
            AzureServiceBusSupportOrdering = _supportOrdering,
            AzureServiceBusUserMetadata = _metadata,
            NumberOfInstances = _numberOfInstances ?? 1
        };
    }
}