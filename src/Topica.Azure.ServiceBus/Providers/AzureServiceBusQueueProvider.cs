using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Consumers;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Producers;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Providers;

public class AzureServiceBusQueueProvider(IAzureServiceBusAdministrationClientProvider administrationClientProvider, IAzureServiceBusClientProvider azureServiceBusClientProvider, IMessageHandlerExecutor messageHandlerExecutor, ILogger<AzureServiceBusQueueProvider> logger) : IQueueProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.AzureServiceBus;

    public async Task CreateQueueAsync(MessagingSettings settings, CancellationToken cancellationToken)
    {
        var queueOptions = new CreateQueueOptions(settings.Source)
        {
            DuplicateDetectionHistoryTimeWindow = TimeSpan.TryParse(settings.AzureServiceBusDuplicateDetectionHistoryTimeWindow!, out var duplicateDetectionHistoryTimeWindowTopic) ? duplicateDetectionHistoryTimeWindowTopic : TimeSpan.FromMinutes(1), // Default
            EnableBatchedOperations = settings.AzureServiceBusEnableBatchedOperations ?? true, // Default
            EnablePartitioning = settings.AzureServiceBusEnablePartitioning ?? false, // Default
            MaxSizeInMegabytes = settings.AzureServiceBusMaxSizeInMegabytes ?? 1024, // Default
            UserMetadata = settings.AzureServiceBusUserMetadata ?? "", // Default
            MaxMessageSizeInKilobytes = settings.AzureServiceBusMaxMessageSizeInKilobytes ?? 256, // Default - 256 KB (standard tier) or 100 MB (premium tier)
            Status = settings.AzureServiceBusEnabledStatus.HasValue && !settings.AzureServiceBusEnabledStatus.Value ? EntityStatus.Disabled : EntityStatus.Active, // Default
        };

        queueOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims", [AccessRights.Manage, AccessRights.Send, AccessRights.Listen]));

        if (!await administrationClientProvider.AdminClient.QueueExistsAsync(settings.Source, cancellationToken))
        {
            await administrationClientProvider.AdminClient.CreateQueueAsync(queueOptions, cancellationToken);
            logger.LogInformation("**** CREATED: Queue: {QueueName}", settings.Source);
        }
        else
        {
            logger.LogInformation("**** EXISTS: Queue: {QueueName} already exists!", settings.Source);
        }
    }

    public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new AzureServiceBusConsumer(azureServiceBusClientProvider, messageHandlerExecutor, messagingSettings, logger);
    }

    public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new AzureServiceBusProducer(producerName, azureServiceBusClientProvider, messagingSettings);
    }
}