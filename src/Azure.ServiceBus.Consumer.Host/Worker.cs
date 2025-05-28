using System.Threading;
using System.Threading.Tasks;
using Azure.ServiceBus.Consumer.Host.Handlers.V1;
using Azure.ServiceBus.Consumer.Host.Messages.V1;
using Azure.ServiceBus.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Azure.ServiceBus.Contracts;

namespace Azure.ServiceBus.Consumer.Host;

public class Worker(IAzureServiceBusTopicFluentBuilder builder, AzureServiceBusConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder
                .WithWorkerName(nameof(PriceSubmittedMessageV1))
                .WithTopicName(settings.PriceSubmittedTopicSettings.Source!)
                .WithSubscriptions(settings.PriceSubmittedTopicSettings.Subscriptions!)
                .WithTimings
                (
                    settings.PriceSubmittedTopicSettings.AutoDeleteOnIdle,
                    settings.PriceSubmittedTopicSettings.DefaultMessageTimeToLive,
                    settings.PriceSubmittedTopicSettings.DuplicateDetectionHistoryTimeWindow
                )
                .WithOptions
                (
                    settings.PriceSubmittedTopicSettings.EnableBatchedOperations,
                    settings.PriceSubmittedTopicSettings.EnablePartitioning,
                    settings.PriceSubmittedTopicSettings.MaxSizeInMegabytes,
                    settings.PriceSubmittedTopicSettings.RequiresDuplicateDetection,
                    settings.PriceSubmittedTopicSettings.MaxMessageSizeInKilobytes,
                    settings.PriceSubmittedTopicSettings.EnabledStatus,
                    settings.PriceSubmittedTopicSettings.SupportOrdering
                )
                .WithMetadata(settings.PriceSubmittedTopicSettings.UserMetadata)
                .BuildConsumerAsync(
                    settings.PriceSubmittedTopicSettings.SubscribeToSource,
                    settings.PriceSubmittedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<PriceSubmittedMessageHandlerV1>(stoppingToken);

        await (await builder
                .WithWorkerName(nameof(QuantityUpdatedMessageV1))
                .WithTopicName(settings.QuantityUpdatedTopicSettings.Source!)
                .WithSubscriptions(settings.QuantityUpdatedTopicSettings.Subscriptions!)
                .WithTimings
                (
                    settings.QuantityUpdatedTopicSettings.AutoDeleteOnIdle,
                    settings.QuantityUpdatedTopicSettings.DefaultMessageTimeToLive,
                    settings.QuantityUpdatedTopicSettings.DuplicateDetectionHistoryTimeWindow
                )
                .WithOptions
                (
                    settings.QuantityUpdatedTopicSettings.EnableBatchedOperations,
                    settings.QuantityUpdatedTopicSettings.EnablePartitioning,
                    settings.QuantityUpdatedTopicSettings.MaxSizeInMegabytes,
                    settings.QuantityUpdatedTopicSettings.RequiresDuplicateDetection,
                    settings.QuantityUpdatedTopicSettings.MaxMessageSizeInKilobytes,
                    settings.QuantityUpdatedTopicSettings.EnabledStatus,
                    settings.QuantityUpdatedTopicSettings.SupportOrdering
                )
                .WithMetadata(settings.QuantityUpdatedTopicSettings.UserMetadata)
                .BuildConsumerAsync(
                    settings.QuantityUpdatedTopicSettings.SubscribeToSource,
                    settings.QuantityUpdatedTopicSettings.NumberOfInstances,
                    stoppingToken
                ))
            .ConsumeAsync<QuantityUpdatedMessageHandlerV1>(stoppingToken);
    }
}