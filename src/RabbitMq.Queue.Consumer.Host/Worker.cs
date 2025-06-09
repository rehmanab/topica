using Microsoft.Extensions.Hosting;
using Topica.RabbitMq.Contracts;

namespace RabbitMq.Queue.Consumer.Host;

public class Worker(IRabbitMqQueueBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}