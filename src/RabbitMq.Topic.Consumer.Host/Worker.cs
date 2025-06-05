using Microsoft.Extensions.Hosting;
using RabbitMq.Topic.Consumer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Topic.Consumer.Host;

public class Worker(IRabbitMqTopicCreationBuilder builder, RabbitMqConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithTopicName(settings.WebAnalyticsTopicSettings.Source)
            .WithSubscribedQueues(settings.WebAnalyticsTopicSettings.WithSubscribedQueues)
            .WithQueueToSubscribeTo(settings.WebAnalyticsTopicSettings.SubscribeToSource)
            .BuildConsumerAsync(settings.WebAnalyticsTopicSettings.NumberOfInstances, stoppingToken);

        await consumer1.ConsumeAsync(stoppingToken);
    }
}