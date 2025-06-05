using Microsoft.Extensions.Hosting;
using RabbitMq.Queue.Consumer.Host.Settings;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Queue.Consumer.Host;

public class Worker(IRabbitMqQueueCreationBuilder builder, RabbitMqConsumerSettings settings) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer1 = await builder
            .WithWorkerName(settings.WebAnalyticsTopicSettings.WorkerName)
            .WithQueueName(settings.WebAnalyticsTopicSettings.Source)
            .BuildConsumerAsync(settings.WebAnalyticsTopicSettings.NumberOfInstances, stoppingToken);

        await consumer1.ConsumeAsync(stoppingToken);
    }
}