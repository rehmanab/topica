using Aws.Topic.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Contracts;
using Topica.Host.Shared.Messages.V1;
using Topica.Messages;

namespace Aws.Topic.Producer.Host;

public class Worker(IAwsTopicCreationBuilder builder, IAwsTopicService awsTopicService, AwsProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    private IProducer _producer1 = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscribedQueues(settings.WebAnalyticsTopicSettings.WithSubscribedQueues)
            .WithQueueToSubscribeTo(settings.WebAnalyticsTopicSettings.SubscribeToSource)
            .WithErrorQueueSettings(
                settings.WebAnalyticsTopicSettings.BuildWithErrorQueue,
                settings.WebAnalyticsTopicSettings.ErrorQueueMaxReceiveCount
            )
            .WithFifoSettings(
                settings.WebAnalyticsTopicSettings.IsFifoQueue,
                settings.WebAnalyticsTopicSettings.IsFifoContentBasedDeduplication
            )
            .WithTemporalSettings(
                settings.WebAnalyticsTopicSettings.MessageVisibilityTimeoutSeconds,
                settings.WebAnalyticsTopicSettings.QueueMessageDelaySeconds,
                settings.WebAnalyticsTopicSettings.QueueMessageRetentionPeriodSeconds,
                settings.WebAnalyticsTopicSettings.QueueReceiveMessageWaitTimeSeconds
            )
            .WithQueueSettings(settings.WebAnalyticsTopicSettings.QueueMaximumMessageSize)
            .BuildProducerAsync(stoppingToken);
        
        var topicArns = awsTopicService.GetAllTopics(settings.WebAnalyticsTopicSettings.Source, settings.WebAnalyticsTopicSettings.IsFifoQueue).ToBlockingEnumerable(cancellationToken: stoppingToken).SelectMany(x => x).ToList();

        switch (topicArns.Count)
        {
            case 0:
                throw new Exception($"No topic found for prefix: {settings.WebAnalyticsTopicSettings.Source}");
            case > 1:
                throw new Exception($"More than 1 topic found for prefix: {settings.WebAnalyticsTopicSettings.Source}");
        }
        
        var topicArn = topicArns.First().TopicArn;
        
        var count = await SendSingleAsync(topicArn, stoppingToken);
        // var count = await SendBatchAsync(topicArn, stoppingToken);

        await _producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
    
    private async Task<int> SendSingleAsync(string topicArn, CancellationToken stoppingToken)
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
            
            await _producer1.ProduceAsync(topicArn, message, messageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced single message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(10, stoppingToken);
        }

        return count;
    }
    
    private async Task<int> SendBatchAsync(string topicArn, CancellationToken stoppingToken)
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
        
        await _producer1.ProduceBatchAsync(topicArn, messages, messageAttributes, cancellationToken: stoppingToken);
            
        logger.LogInformation("Produced ({Count}) batch messages in groups of 10 for AWS to {MessagingSettingsSource}", messages.Count, settings.WebAnalyticsTopicSettings.Source);

        return messages.Count;
    }
}