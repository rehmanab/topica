using Aws.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Consumer.Host;

public class Worker(IAwsTopicFluentBuilder builder, AwsConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer1 = await builder
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
            .BuildConsumerAsync(
                settings.WebAnalyticsTopicSettings.NumberOfInstances,
                settings.WebAnalyticsTopicSettings.QueueReceiveMaximumNumberOfMessages,
                stoppingToken
            );

        await consumer1.ConsumeAsync(stoppingToken);
    }
}