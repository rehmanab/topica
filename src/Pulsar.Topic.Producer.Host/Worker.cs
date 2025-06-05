using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulsar.Topic.Producer.Host.Settings;
using Topica.Contracts;
using Topica.Host.Shared.Messages.V1;
using Topica.Messages;
using Topica.Pulsar.Contracts;

namespace Pulsar.Topic.Producer.Host;

public class Worker(IPulsarTopicCreationBuilder builder, PulsarProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder
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

        var count = await SendSingleAsync(stoppingToken);
        // var count = await SendBatchAsync(stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new FileDownloadedMessageV1 { ConversationId = Guid.NewGuid(), EventId = count, EventName = "file.downloaded.web.v1", Type = nameof(FileDownloadedMessageV1) };
            
            await _producer1.ProduceAsync(settings.WebAnalyticsTopicSettings.Source, message, null, stoppingToken);
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(CancellationToken stoppingToken)
    {
        var messages = Enumerable.Range(1, 5000)
            .Select(index => new FileDownloadedMessageV1 { ConversationId = Guid.NewGuid(), EventId = index, EventName = "file.downloaded.web.v1", Type = nameof(FileDownloadedMessageV1) })
            .Cast<BaseMessage>()
            .ToList();

        await _producer1.ProduceBatchAsync(settings.WebAnalyticsTopicSettings.Source, messages, null, stoppingToken);
        logger.LogInformation("Sent batch to {Topic}: {Count}", settings.WebAnalyticsTopicSettings.Source, messages.Count);

        return messages.Count;
    }
}