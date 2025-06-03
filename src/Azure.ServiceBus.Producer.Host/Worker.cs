using Azure.ServiceBus.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Azure.ServiceBus.Contracts;
using Topica.Host.Shared.Messages.V1;

namespace Azure.ServiceBus.Producer.Host;

public class Worker(IAzureServiceBusTopicFluentBuilder builder, AzureServiceBusProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer1 = await builder
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
            .BuildProducerAsync(stoppingToken);
        

        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new VideoPlayedMessageV1
            {
                EventId = count,
                EventName = "video.played.web.v1",
                ConversationId = Guid.NewGuid(),
                Type = nameof(VideoPlayedMessageV1),
                RaisingComponent = settings.WebAnalyticsTopicSettings.WorkerName,
                Version = "V1",
                AdditionalProperties = new Dictionary<string, string> { { "prop1", "value1" } }
            };
            
            await producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, stoppingToken);
            logger.LogInformation("Sent to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, count);
            count++;

            await Task.Delay(1000, stoppingToken);
        }
    }
}