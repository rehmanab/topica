using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Producer.Host.Settings;
using Topica.Host.Shared.Messages.V1;
using Topica.Pulsar.Contracts;

namespace Pulsar.Producer.Host;

public class Worker(IPulsarConsumerTopicFluentBuilder builder, PulsarProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithConsumerGroup(settings.WebAnalyticsTopicSettings.ConsumerGroup)
            .WithConfiguration(
                settings.WebAnalyticsTopicSettings.Tenant,
                settings.WebAnalyticsTopicSettings.Namespace,
                settings.WebAnalyticsTopicSettings.NumberOfPartitions
            )
            .WithTopicOptions(settings.WebAnalyticsTopicSettings.StartNewConsumerEarliest)
            .WithProducerOptions(
                settings.WebAnalyticsTopicSettings.BlockIfQueueFull,
                settings.WebAnalyticsTopicSettings.MaxPendingMessages,
                settings.WebAnalyticsTopicSettings.MaxPendingMessagesAcrossPartitions,
                settings.WebAnalyticsTopicSettings.EnableBatching,
                settings.WebAnalyticsTopicSettings.EnableChunking,
                settings.WebAnalyticsTopicSettings.BatchingMaxMessages,
                settings.WebAnalyticsTopicSettings.BatchingMaxPublishDelayMilliseconds
            )
            .BuildProducerAsync(stoppingToken);

        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new FileDownloadedMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "file.downloaded.web.v1", Type = nameof(FileDownloadedMessageV1) };

            await producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, stoppingToken);
            
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        await producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
}