using Microsoft.Extensions.Hosting;
using Topica.Pulsar.Contracts;

namespace Pulsar.Topic.Consumer.Host;

public class Worker(IPulsarTopicBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}