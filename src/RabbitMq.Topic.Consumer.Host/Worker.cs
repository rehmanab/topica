using Microsoft.Extensions.Hosting;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Topic.Consumer.Host;

public class Worker(IRabbitMqTopicBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}