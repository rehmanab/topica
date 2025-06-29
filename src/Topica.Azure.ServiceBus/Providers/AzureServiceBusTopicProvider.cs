using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Consumers;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Azure.ServiceBus.Producers;
using Topica.Contracts;
using Topica.Settings;

namespace Topica.Azure.ServiceBus.Providers;

public class AzureServiceBusTopicProvider(IAzureServiceBusAdministrationClientProvider administrationClientProvider, IAzureServiceBusClientProvider azureServiceBusClientProvider, IMessageHandlerExecutor messageHandlerExecutor, ILogger<AzureServiceBusTopicProvider> logger) : ITopicProvider
{
    public MessagingPlatform MessagingPlatform => MessagingPlatform.AzureServiceBus;

    public async Task CreateTopicAsync(MessagingSettings settings, CancellationToken cancellationToken)
    {
        var topicOptions = new CreateTopicOptions(settings.Source)
        {
            AutoDeleteOnIdle = TimeSpan.TryParse(settings.AzureServiceBusAutoDeleteOnIdle!, out var autoDeleteOnIdleTopic) ? autoDeleteOnIdleTopic : TimeSpan.MaxValue, // Default
            DefaultMessageTimeToLive = TimeSpan.TryParse(settings.AzureServiceBusDefaultMessageTimeToLive!, out var defaultMessageTimeToLiveTopic) ? defaultMessageTimeToLiveTopic : TimeSpan.MaxValue, // Default
            DuplicateDetectionHistoryTimeWindow = TimeSpan.TryParse(settings.AzureServiceBusDuplicateDetectionHistoryTimeWindow!, out var duplicateDetectionHistoryTimeWindowTopic) ? duplicateDetectionHistoryTimeWindowTopic : TimeSpan.FromMinutes(1), // Default
            EnableBatchedOperations = settings.AzureServiceBusEnableBatchedOperations ?? true, // Default
            EnablePartitioning = settings.AzureServiceBusEnablePartitioning ?? false, // Default
            MaxSizeInMegabytes = settings.AzureServiceBusMaxSizeInMegabytes ?? 1024, // Default
            RequiresDuplicateDetection = settings.AzureServiceBusRequiresDuplicateDetection ?? true,
            UserMetadata = settings.AzureServiceBusUserMetadata ?? "", // Default
            MaxMessageSizeInKilobytes = settings.AzureServiceBusMaxMessageSizeInKilobytes ?? 256, // Default - 256 KB (standard tier) or 100 MB (premium tier)
            Status = settings.AzureServiceBusEnabledStatus.HasValue && !settings.AzureServiceBusEnabledStatus.Value ? EntityStatus.Disabled : EntityStatus.Active, // Default
            SupportOrdering = settings.AzureServiceBusSupportOrdering ?? false // Default
        };

        topicOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims", [AccessRights.Manage, AccessRights.Send, AccessRights.Listen]));

        if (!await administrationClientProvider.AdminClient.TopicExistsAsync(settings.Source, cancellationToken))
        {
            await administrationClientProvider.AdminClient.CreateTopicAsync(topicOptions, cancellationToken);
            logger.LogInformation("**** CREATED: topic: {TopicName}", settings.Source);
        }
        else
        {
            logger.LogInformation("**** EXISTS: Topic: {TopicName} already exists!", settings.Source);
        }

        foreach (var subscription in settings.AzureServiceBusSubscriptions)
        {
            if (await administrationClientProvider.AdminClient.SubscriptionExistsAsync(settings.Source, subscription.Source, cancellationToken))
            {
                logger.LogInformation("**** EXISTS: Subscription: {Subscription} for Topic: {TopicName} already exists!", subscription.Source, settings.Source);
                continue;
            }

            var subscriptionOptions = new CreateSubscriptionOptions(settings.Source, subscription.Source)
            {
                AutoDeleteOnIdle = TimeSpan.TryParse(subscription.AutoDeleteOnIdle, out var autoDeleteOnIdleSubscription) ? autoDeleteOnIdleSubscription : TimeSpan.MaxValue, // Default
                DefaultMessageTimeToLive = TimeSpan.TryParse(subscription.DefaultMessageTimeToLive, out var defaultMessageTimeToLiveSubscription) ? defaultMessageTimeToLiveSubscription : TimeSpan.MaxValue, // Default
                EnableBatchedOperations = subscription.EnableBatchedOperations ?? true, // Default
                UserMetadata = subscription.UserMetadata ?? "",
                DeadLetteringOnMessageExpiration = subscription.DeadLetteringOnMessageExpiration ?? false, // Default
                EnableDeadLetteringOnFilterEvaluationExceptions = subscription.EnableDeadLetteringOnFilterEvaluationExceptions ?? true, // Default
                ForwardDeadLetteredMessagesTo = subscription.ForwardDeadLetteredMessagesTo ?? null, // Default
                ForwardTo = subscription.ForwardTo ?? null, // Default
                LockDuration = TimeSpan.TryParse(subscription.LockDuration, out var lockDurationSubscription) ? lockDurationSubscription : TimeSpan.FromSeconds(60), // Default
                MaxDeliveryCount = subscription.MaxDeliveryCount ?? 10, // Default
                RequiresSession = subscription.RequiresSession ?? false, // Default
                Status = subscription.EnabledStatus.HasValue && !subscription.EnabledStatus.Value ? EntityStatus.Disabled : EntityStatus.Active, // Default
            };

            _ = await administrationClientProvider.AdminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken);
            logger.LogInformation("**** CREATED: subscription: {SubscriptionName} for topic: {TopicName}", subscription.Source, settings.Source);
        }
    }

    public async Task<IConsumer> ProvideConsumerAsync(MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new AzureServiceBusTopicSubscriptionConsumer(azureServiceBusClientProvider, messageHandlerExecutor, messagingSettings, logger);
    }

    public async Task<IProducer> ProvideProducerAsync(string producerName, MessagingSettings messagingSettings)
    {
        await Task.CompletedTask;

        return new AzureServiceBusTopicProducer(producerName, azureServiceBusClientProvider);
    }
}