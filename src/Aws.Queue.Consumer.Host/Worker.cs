using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Queue.Consumer.Host;

public class Worker(IAwsQueueBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}