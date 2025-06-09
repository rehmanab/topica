using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Topica.Azure.ServiceBus.Contracts;

namespace Azure.ServiceBus.Topic.Consumer.Host;

public class Worker(IAzureServiceBusTopicBuilder builder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await (await builder.BuildConsumerAsync(stoppingToken)).ConsumeAsync(stoppingToken);
    }
}