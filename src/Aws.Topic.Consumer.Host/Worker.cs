using Aws.Topic.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Topic.Consumer.Host;

public class Worker(IAwsTopicCreationBuilder topicCreationBuilder, AwsConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await topicCreationBuilder
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
            .BuildConsumerAsync(
                settings.WebAnalyticsTopicSettings.NumberOfInstances,
                settings.WebAnalyticsTopicSettings.QueueReceiveMaximumNumberOfMessages,
                stoppingToken
            )).ConsumeAsync(stoppingToken);
    }
}