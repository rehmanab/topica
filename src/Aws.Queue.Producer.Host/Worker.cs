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
        
        var queueUrl = await awsQueueService.GetQueueUrlAsync((settings.WebAnalyticsQueueSettings.IsFifoQueue ?? false) && !settings.WebAnalyticsQueueSettings.Source.EndsWith(Constants.FifoSuffix) ? $"{settings.WebAnalyticsQueueSettings.Source}{Constants.FifoSuffix}" : settings.WebAnalyticsQueueSettings.Source);
        
        if (queueUrl == null)
        {
            throw new Exception($"No queue found for name: {settings.WebAnalyticsQueueSettings.Source}");
        }
        
        var count = await SendSingleAsync(queueUrl, stoppingToken);
        // var count = await SendBatchAsync(queueUrl, stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }

    private async Task<int> SendSingleAsync(string queueUrl, CancellationToken stoppingToken)
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
            
            await _producer1.ProduceAsync(queueUrl, message, messageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsQueueSettings.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(10, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string queueUrl, CancellationToken stoppingToken)
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
        
        await _producer1.ProduceBatchAsync(queueUrl, messages, messageAttributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, settings.WebAnalyticsQueueSettings.Source);

        return messages.Count;
    }
}