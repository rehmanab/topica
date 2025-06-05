using Aws.Queue.Consumer.Host.Settings;
using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Queue.Consumer.Host;

public class Worker(IAwsQueueCreationBuilder queueCreationBuilder, AwsConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await queueCreationBuilder
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
            .BuildConsumerAsync(
                settings.WebAnalyticsQueueSettings.NumberOfInstances,
                settings.WebAnalyticsQueueSettings.QueueReceiveMaximumNumberOfMessages,
                stoppingToken
            )).ConsumeAsync(stoppingToken);
    }
}