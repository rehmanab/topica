using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Providers;

public class AzureServiceBusTopicProvider(ServiceBusAdministrationClient adminClient, ILogger<AzureServiceBusTopicProvider> logger) : ITopicProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.AzureServiceBus;
    
    public async Task CreateTopicAsync(ConsumerSettings settings)
    {
        await CreateTopicAsync(
            settings.Source!,
            settings.AzureServiceBusAutoDeleteOnIdle!,
            settings.AzureServiceBusDefaultMessageTimeToLive!,
            settings.AzureServiceBusDuplicateDetectionHistoryTimeWindow!,
            settings.AzureServiceBusEnableBatchedOperations,
            settings.AzureServiceBusEnablePartitioning,
            settings.AzureServiceBusMaxSizeInMegabytes,
            settings.AzureServiceBusRequiresDuplicateDetection,
            settings.AzureServiceBusUserMetadata,
            settings.AzureServiceBusMaxMessageSizeInKilobytes,
            settings.AzureServiceBusEnabledStatus,
            settings.AzureServiceBusSupportOrdering,
            settings.AzureServiceBusSubscriptions
        );
    }

    public async Task CreateTopicAsync(ProducerSettings settings)
    {
        await CreateTopicAsync(
            settings.Source,
            settings.AzureServiceBusAutoDeleteOnIdle!,
            settings.AzureServiceBusDefaultMessageTimeToLive!,
            settings.AzureServiceBusDuplicateDetectionHistoryTimeWindow!,
            settings.AzureServiceBusEnableBatchedOperations,
            settings.AzureServiceBusEnablePartitioning,
            settings.AzureServiceBusMaxSizeInMegabytes,
            settings.AzureServiceBusRequiresDuplicateDetection,
            settings.AzureServiceBusUserMetadata,
            settings.AzureServiceBusMaxMessageSizeInKilobytes,
            settings.AzureServiceBusEnabledStatus,
            settings.AzureServiceBusSupportOrdering,
            settings.AzureServiceBusSubscriptions
        );
    }
    
    private async Task CreateTopicAsync(
        string source, 
        string azureServiceBusAutoDeleteOnIdle,
        string azureServiceBusDefaultMessageTimeToLive,
        string azureServiceBusDuplicateDetectionHistoryTimeWindow,
        bool? azureServiceBusEnableBatchedOperations,
        bool? azureServiceBusEnablePartitioning,
        int? azureServiceBusMaxSizeInMegabytes,
        bool? azureServiceBusRequiresDuplicateDetection,
        string? azureServiceBusUserMetadata,
        int? azureServiceBusMaxMessageSizeInKilobytes,
        bool? azureServiceBusEnabledStatus,
        bool? azureServiceBusSupportOrdering,
        AzureServiceBusTopicSubscriptionSettings[]? azureServiceBusSubscriptions
        )
    {
        var topicOptions = new CreateTopicOptions(source)
        {
            AutoDeleteOnIdle = TimeSpan.TryParse(azureServiceBusAutoDeleteOnIdle, out var autoDeleteOnIdleTopic) ? autoDeleteOnIdleTopic : TimeSpan.MaxValue, // Default
            DefaultMessageTimeToLive = TimeSpan.TryParse(azureServiceBusDefaultMessageTimeToLive, out var defaultMessageTimeToLiveTopic) ? defaultMessageTimeToLiveTopic : TimeSpan.MaxValue, // Default
            DuplicateDetectionHistoryTimeWindow = TimeSpan.TryParse(azureServiceBusDuplicateDetectionHistoryTimeWindow, out var duplicateDetectionHistoryTimeWindowTopic) ? duplicateDetectionHistoryTimeWindowTopic : TimeSpan.FromMinutes(1), // Default
            EnableBatchedOperations = azureServiceBusEnableBatchedOperations ?? true, // Default
            EnablePartitioning = azureServiceBusEnablePartitioning ?? false, // Default
            MaxSizeInMegabytes = azureServiceBusMaxSizeInMegabytes ?? 1024, // Default
            RequiresDuplicateDetection = azureServiceBusRequiresDuplicateDetection ?? true,
            UserMetadata = azureServiceBusUserMetadata,
            MaxMessageSizeInKilobytes = azureServiceBusMaxMessageSizeInKilobytes ?? 256, // Default - 256 KB (standard tier) or 100 MB (premium tier)
            Status = azureServiceBusEnabledStatus.HasValue && !azureServiceBusEnabledStatus.Value ? EntityStatus.Disabled : EntityStatus.Active, // Default
            SupportOrdering = azureServiceBusSupportOrdering ?? false // Default
        };
        
        topicOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims", [AccessRights.Manage, AccessRights.Send, AccessRights.Listen]));

        if(!await adminClient.TopicExistsAsync(source))
        {
            await adminClient.CreateTopicAsync(topicOptions);
            logger.LogInformation("Created topic: {TopicName}", source);
        }
        else
        {
            logger.LogInformation("Topic: {TopicName} already exists!", source);
        }
        
        foreach (var subscription in azureServiceBusSubscriptions!)
        {
            if (await adminClient.SubscriptionExistsAsync(source, subscription.Source))
            {
                logger.LogInformation("Subscription: {Subscription} for Topic: {TopicName} already exists!", subscription.Source, source);
                continue;
            }
		
            var subscriptionOptions = new CreateSubscriptionOptions(source, subscription.Source)
            {
                AutoDeleteOnIdle = TimeSpan.TryParse(subscription.AutoDeleteOnIdle, out var autoDeleteOnIdleSubscription) ? autoDeleteOnIdleSubscription : TimeSpan.MaxValue, // Default
                DefaultMessageTimeToLive = TimeSpan.TryParse(subscription.DefaultMessageTimeToLive, out var defaultMessageTimeToLiveSubscription) ? defaultMessageTimeToLiveSubscription :TimeSpan.MaxValue, // Default
                EnableBatchedOperations = subscription.EnableBatchedOperations ?? true, // Default
                UserMetadata = subscription.UserMetadata,
                DeadLetteringOnMessageExpiration = subscription.DeadLetteringOnMessageExpiration ?? false, // Default
                EnableDeadLetteringOnFilterEvaluationExceptions = subscription.EnableDeadLetteringOnFilterEvaluationExceptions ?? true, // Default
                ForwardDeadLetteredMessagesTo = subscription.ForwardDeadLetteredMessagesTo ?? null, // Default
                ForwardTo = subscription.ForwardTo ?? null, // Default
                LockDuration = TimeSpan.TryParse(subscription.LockDuration, out var lockDurationSubscription) ? lockDurationSubscription :TimeSpan.FromSeconds(60), // Default
                MaxDeliveryCount = subscription.MaxDeliveryCount ?? 10, // Default
                RequiresSession = subscription.RequiresSession ?? false, // Default
                Status = subscription.EnabledStatus.HasValue && !subscription.EnabledStatus.Value ? EntityStatus.Disabled : EntityStatus.Active, // Default
            };

            var createdSubscription = await adminClient.CreateSubscriptionAsync(subscriptionOptions);
            logger.LogInformation("Created subscription: {SubscriptionName} for topic: {TopicName} - Result: {Result}", subscription.Source, source, createdSubscription.Value.Status);
        }
    }
}