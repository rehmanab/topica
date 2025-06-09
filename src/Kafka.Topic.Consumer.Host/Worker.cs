using Microsoft.Extensions.Hosting;
using Topica.Kafka.Contracts;

namespace Kafka.Topic.Consumer.Host;

public class Worker(IKafkaTopicBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}