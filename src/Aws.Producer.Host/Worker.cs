using Aws.Producer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Topica.Aws.Contracts;
using Topica.Host.Shared.Messages.V1;

namespace Aws.Producer.Host;

public class Worker(IAwsTopicFluentBuilder builder, IAwsTopicService awsTopicService, AwsProducerSettings settings, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscribedQueues(
                settings.WebAnalyticsTopicSettings.SubscribeToSource,
                settings.WebAnalyticsTopicSettings.WithSubscribedQueues
            )
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
        
        var count = 1;
        while(!stoppingToken.IsCancellationRequested)
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

            var awsMessageAttributes = new Dictionary<string, string>
            {
                {"SignatureVersion", "2" }
            };
            await producer1.ProduceAsync(topicArn, message, awsMessageAttributes, cancellationToken: stoppingToken);
            
            logger.LogInformation("Produced message to {MessagingSettingsSource}: {MessageIdName}", settings.WebAnalyticsTopicSettings.Source, $"{message.EventId} : {message.EventName}");
            
            count++;

            await Task.Delay(10, stoppingToken);
        }

        await producer1.DisposeAsync();

        logger.LogInformation("Finished: {Count} messages sent", count);
    }
}