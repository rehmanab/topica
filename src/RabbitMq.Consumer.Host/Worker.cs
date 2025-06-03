using Microsoft.Extensions.Hosting;
using RabbitMq.Consumer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Consumer.Host;

public class Worker(IRabbitMqTopicFluentBuilder builder, RabbitMqConsumerSettings settings) : BackgroundService
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
            .BuildConsumerAsync(
                settings.WebAnalyticsTopicSettings.NumberOfInstances,
                stoppingToken
            );

        await consumer1.ConsumeAsync(stoppingToken);
    }
}