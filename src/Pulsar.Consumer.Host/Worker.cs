using System.Reflection;
using Microsoft.Extensions.Hosting;
using Topica.Contracts;
using Topica.Settings;

namespace Pulsar.Consumer.Host;

public class Worker(IConsumer consumer, IEnumerable<ConsumerSettings> consumerSettings) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var consumerSetting in consumerSettings)
        {
            // Pulsar different consumer names will read the topic from the start and independently - i.e consumer name is like a consumer group
            var consumerName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{consumerSetting.MessageToHandle}";
            await consumer.ConsumeAsync(consumerName, consumerSetting, stoppingToken);
        }
    }
}