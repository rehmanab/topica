using Aws.Queue.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Aws.Helpers;
using Topica.Contracts;
using Topica.Host.Shared.Messages.V1;
using Topica.Messages;

namespace Aws.Queue.Producer.Host;

public class Worker(IAwsQueueCreationBuilder queueCreationBuilder, IAwsQueueService awsQueueService, AwsProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await queueCreationBuilder
            .WithWorkerName(settings.WebAnalyticsQueueSettings.WorkerName)
            .WithQueueName(settings.WebAnalyticsQueueSettings.Source)
            .WithErrorQueueSettings(
                settings.WebAnalyticsQueueSettings.BuildWithErrorQueue,
                settings.WebAnalyticsQueueSettings.ErrorQueueMaxReceiveCount
            )
            .WithFifoSettings(
                settings.WebAnalyticsQueueSettings.IsFifoQueue,
                settings.WebAnalyticsQueueSettings.IsFifoContentBasedDeduplication
            )
            .WithTemporalSettings(
                settings.WebAnalyticsQueueSettings.MessageVisibilityTimeoutSeconds,
                settings.WebAnalyticsQueueSettings.QueueMessageDelaySeconds,
                settings.WebAnalyticsQueueSettings.QueueMessageRetentionPeriodSeconds,
                settings.WebAnalyticsQueueSettings.QueueReceiveMessageWaitTimeSeconds
            )
            .WithQueueSettings(settings.WebAnalyticsQueueSettings.QueueMaximumMessageSize)
            .BuildProducerAsync(stoppingToken);
        
        var count = await SendSingleAsync(settings.WebAnalyticsQueueSettings.Source, stoppingToken);
        // var count = await SendBatchAsync(settings.WebAnalyticsQueueSettings.Source, stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }

    private async Task<int> SendSingleAsync(string queueName, CancellationToken stoppingToken)
    {
        var count = 1;
        while (!stoppingToken.IsCancellationRequested)
        {
            var messageGroupId = Guid.NewGuid().ToString();
            
            var message = new ButtonClickedMessageV1
            {
                ConversationId = Guid.NewGuid(), 
                EventId = count, 
                EventName = "button.clicked.web.v1", 
                Type = nameof(ButtonClickedMessageV1),
                MessageGroupId = messageGroupId
            };

            var messageAttributes = new Dictionary<string, string>
            {
                {"SignatureVersion", "2" }
            };
            
            await _producer1.ProduceAsync(queueName, message, messageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", TopicQueueHelper.AddTopicQueueNameFifoSuffix(settings.WebAnalyticsQueueSettings.Source, settings.WebAnalyticsQueueSettings.IsFifoQueue ?? false), $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(1000, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string queueName, CancellationToken stoppingToken)
    {
        var messageGroupId = Guid.NewGuid();
            
        var messages = Enumerable.Range(1, 500)
            .Select(index => new ButtonClickedMessageV1
            {
                ConversationId = Guid.NewGuid(),
                EventId = index,
                EventName = "button.clicked.web.v1",
                Type = nameof(ButtonClickedMessageV1),
                MessageGroupId = messageGroupId.ToString()
            })
            .Cast<BaseMessage>()
            .ToList();

        var messageAttributes = new Dictionary<string, string>
        {
            {"SignatureVersion", "2" }
        };
        
        await _producer1.ProduceBatchAsync(queueName, messages, messageAttributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, TopicQueueHelper.AddTopicQueueNameFifoSuffix(settings.WebAnalyticsQueueSettings.Source, settings.WebAnalyticsQueueSettings.IsFifoQueue ?? false));

        return messages.Count;
    }
}