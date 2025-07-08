using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;

namespace Kafka.Topic.Consumer.Host;

public class Worker([FromKeyedServices("Consumer")] IConsumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await consumer.ConsumeAsync(stoppingToken);
    }
}