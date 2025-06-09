using Microsoft.Extensions.Hosting;
using Topica.Aws.Contracts;

namespace Aws.Topic.Consumer.Host;

public class Worker(IAwsTopicBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}