using System.Threading;
using System.Threading.Tasks;
using Azure.ServiceBus.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Azure.ServiceBus.Contracts;

namespace Azure.ServiceBus.Consumer.Host;

public class Worker(IAzureServiceBusTopicFluentBuilder builder, AzureServiceBusConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscriptions(settings.WebAnalyticsTopicSettings.Subscriptions)
            .WithTimings
            (
                settings.WebAnalyticsTopicSettings.AutoDeleteOnIdle,
                settings.WebAnalyticsTopicSettings.DefaultMessageTimeToLive,
                settings.WebAnalyticsTopicSettings.DuplicateDetectionHistoryTimeWindow
            )
            .WithOptions
            (
                settings.WebAnalyticsTopicSettings.EnableBatchedOperations,
                settings.WebAnalyticsTopicSettings.EnablePartitioning,
                settings.WebAnalyticsTopicSettings.MaxSizeInMegabytes,
                settings.WebAnalyticsTopicSettings.RequiresDuplicateDetection,
                settings.WebAnalyticsTopicSettings.MaxMessageSizeInKilobytes,
                settings.WebAnalyticsTopicSettings.EnabledStatus,
                settings.WebAnalyticsTopicSettings.SupportOrdering
            )
            .WithMetadata(settings.WebAnalyticsTopicSettings.UserMetadata)
            .BuildConsumerAsync(
                settings.WebAnalyticsTopicSettings.SubscribeToSource,
                settings.WebAnalyticsTopicSettings.NumberOfInstances,
                stoppingToken
            );

        await consumer1.ConsumeAsync(stoppingToken);
    }
}